import { Component, OnDestroy, OnInit, ChangeDetectorRef} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse, HttpEventType } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { AccountService } from '../../services/account.service';
import { PdfUploadService, UploadHistoryItem } from '../../services/pdf-upload.service';
import { DocumentProcessingHubService } from '../../services/document-processing-hub.service';
import { MonthlyEvolutionChartComponent } from '../../components/monthly-evolution-chart/monthly-evolution-chart';
import { MovementDto } from '../../models/movement';

// Options exposed in the "filter by type" dropdown
type TypeFilter = 'All' | 'C' | 'D';

// Visual state of the bank statement import widget
type StatementUploadState = 'idle' | 'uploading' | 'processing' | 'success' | 'error';

// Number of movements shown per page in the transaction list
const PAGE_SIZE = 10;

@Component({
  selector: 'app-current-account',
  standalone: true,
  imports: [CommonModule, FormsModule, MonthlyEvolutionChartComponent],
  templateUrl: './current-account.html',
  styleUrl: './current-account.scss'
})
export class CurrentAccountComponent implements OnInit, OnDestroy {
  // Full set of movements returned by the backend, ordered oldest -> newest
  public allMovements: MovementDto[] = [];

  // Subset currently rendered (after filtering, before pagination), newest first
  public filteredMovements: MovementDto[] = [];

  // Slice of filteredMovements shown on the current page
  public pagedMovements: MovementDto[] = [];

  // Active type filter selection bound to the dropdown
  public selectedTypeFilter: TypeFilter = 'All';

  // Current account balance, taken from the most recent movement's running balance
  public currentBalance: number | null = null;

  // Sum of all credit ("C") movements currently loaded
  public totalCredits = 0;

  // Sum of all debit ("D") movements currently loaded
  public totalDebits = 0;

  // True while the initial movements request is in flight
  public isLoading = true;

  // Populated with a user-facing message when the movements request fails
  public errorMessage: string | null = null;

  // Tracks which movement row currently has its note field open for editing
  public editingMovementId: string | null = null;

  // Working copy of the note text for the row currently being edited
  public noteDraft = '';

  // True while a note update request is in flight
  public isSavingNote = false;

  // --- Pagination ---
  public currentPage = 1;
  public readonly pageSize = PAGE_SIZE;

  // --- Bank statement (PDF) import ---
  public uploadState: StatementUploadState = 'idle';
  public uploadProgress = 0;
  public uploadErrorMessage: string | null = null;
  private pendingDocumentId: string | null = null;
  private hubSubscription: Subscription | null = null;

  constructor(
    private readonly accountService: AccountService,
    private readonly pdfUploadService: PdfUploadService,
    private readonly documentHub: DocumentProcessingHubService,
    private cdr: ChangeDetectorRef
  ) {}

  public ngOnInit(): void {
    this.loadMovements();

    // Listen for live "documentProcessed" pushes so a statement import submitted from this
    // page reflects success/failure without the user needing to refresh
    this.documentHub.connect();
    this.hubSubscription = this.documentHub.onDocumentProcessed().subscribe((item) => this.onDocumentProcessed(item));
  }

  public ngOnDestroy(): void {
    this.hubSubscription?.unsubscribe();
    this.documentHub.disconnect();
  }

  // Fetch every movement for the account and derive the summary figures shown at the top
  public loadMovements(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.accountService.getMovements().subscribe({
      next: (movements) => {
        queueMicrotask(() => {
        this.allMovements = movements;
        this.computeSummary(movements);
        this.currentPage = 1;
        this.applyFilters();
        this.isLoading = false;
        this.cdr.detectChanges();
        });
      },
      error: () => {
        this.errorMessage = 'Não foi possível carregar os movimentos da conta. Tente novamente mais tarde.';
        this.isLoading = false;
      }
    });
  }

  // Recalculate the balance and totals shown in the summary cards
  private computeSummary(movements: MovementDto[]): void {
    if (movements.length === 0) {
      this.currentBalance = null;
      this.totalCredits = 0;
      this.totalDebits = 0;
      return;
    }

    // The backend returns movements ordered by TransactionDate ascending, so the
    // running balance on the last entry is the current account balance
    this.currentBalance = movements[movements.length - 1].balance;

    this.totalCredits = movements
      .filter((m) => m.type === 'C')
      .reduce((sum, m) => sum + m.amount, 0);

    this.totalDebits = movements
      .filter((m) => m.type === 'D')
      .reduce((sum, m) => sum + m.amount, 0);
  }

  // Apply the active type filter, sort newest-first, and reset pagination to the first page
  public applyFilters(): void {
    const source = this.selectedTypeFilter === 'All'
      ? this.allMovements
      : this.allMovements.filter((m) => m.type === this.selectedTypeFilter);

    this.filteredMovements = [...source].sort(
      (a, b) => new Date(b.transactionDate).getTime() - new Date(a.transactionDate).getTime()
    );

    this.currentPage = 1;
    this.applyPagination();
  }

  // --- Pagination helpers ---

  public get totalPages(): number {
    return Math.max(1, Math.ceil(this.filteredMovements.length / this.pageSize));
  }

  private applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    this.pagedMovements = this.filteredMovements.slice(start, start + this.pageSize);
  }

  public goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) {
      return;
    }
    this.currentPage = page;
    this.applyPagination();
  }

  public getTypeClass(type: 'C' | 'D'): Record<string, boolean> {
    return {
      'badge-credit': type === 'C',
      'badge-debit': type === 'D'
    };
  }

  public getAmountClass(type: 'C' | 'D'): Record<string, boolean> {
    return {
      'amount-positive': type === 'C',
      'amount-negative': type === 'D'
    };
  }

  public getTypeLabel(type: 'C' | 'D'): string {
    return type === 'C' ? 'Crédito' : 'Débito';
  }

  // Open the inline note editor for a given movement
  public startEditingNote(movement: MovementDto): void {
    this.editingMovementId = movement.id;
    this.noteDraft = movement.userNote ?? '';
  }

  public cancelEditingNote(): void {
    this.editingMovementId = null;
    this.noteDraft = '';
  }

  // Persist the edited note for the currently active row
  public saveNote(movement: MovementDto): void {
    const trimmedNote = this.noteDraft.trim();
    this.isSavingNote = true;

    this.accountService.updateMovementNote(movement.id, trimmedNote).subscribe({
      next: () => {
        movement.userNote = trimmedNote;
        this.isSavingNote = false;
        this.editingMovementId = null;
        this.noteDraft = '';
      },
      error: () => {
        this.isSavingNote = false;
        this.errorMessage = 'Não foi possível guardar a nota. Tente novamente.';
      }
    });
  }

  // --- Bank statement (PDF) import ---

  // Triggered by the hidden file input's (change) event
  public onStatementFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';

    if (file) {
      this.uploadStatement(file);
    }
  }

  private uploadStatement(file: File): void {
    this.uploadState = 'uploading';
    this.uploadProgress = 0;
    this.uploadErrorMessage = null;
    this.pendingDocumentId = null;

    this.pdfUploadService.uploadPdf(file, 'ActivoBankStatement').subscribe({
      next: (event) => {
        if (event.type === HttpEventType.UploadProgress && event.total) {
          this.uploadProgress = Math.round((100 * event.loaded) / event.total);
        } else if (event.type === HttpEventType.Response) {
          // Upload accepted by DocumentIngestion; actual parsing happens asynchronously
          // via RabbitMQ/PdfParser and is reported back through the SignalR hub
          this.pendingDocumentId = event.body?.documentId ?? null;
          this.uploadState = 'processing';
        }
      },
      error: (error: HttpErrorResponse) => {
        this.uploadState = 'error';
        this.uploadErrorMessage = error.error?.message ?? 'Falha ao carregar o extrato. Verifique o ficheiro e tente novamente.';
      }
    });
  }

  // Handle a live "documentProcessed" push from DocumentIngestion for the statement we submitted
  private onDocumentProcessed(item: UploadHistoryItem): void {
    if (!this.pendingDocumentId || item.id !== this.pendingDocumentId) {
      return;
    }

    if (item.success) {
      this.uploadState = 'success';
      // Refresh the movement list and balance so the newly imported statement is reflected
      this.loadMovements();
    } else {
      this.uploadState = 'error';
      this.uploadErrorMessage = item.errorMessage ?? 'O extrato não pôde ser processado.';
    }
  }

  public dismissUploadStatus(): void {
    this.uploadState = 'idle';
    this.uploadProgress = 0;
    this.uploadErrorMessage = null;
    this.pendingDocumentId = null;
  }
}
