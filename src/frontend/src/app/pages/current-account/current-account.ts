import { Component, OnDestroy, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse, HttpEventType } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { AccountService } from '../../services/account.service';
import { PdfUploadService, UploadHistoryItem } from '../../services/pdf-upload.service';
import { DocumentProcessingHubService } from '../../services/document-processing-hub.service';
import { MonthlyEvolutionChartComponent } from '../../components/monthly-evolution-chart/monthly-evolution-chart';
import { DateFilterField, MovementDto, MovementQueryParams } from '../../models/movement';

// Which date field the active date-range filter applies to
type DateFilterMode = 'transactionDate' | 'valueDate';

// Visual state of the bank statement import widget
type StatementUploadState = 'idle' | 'uploading' | 'processing' | 'success' | 'error';

// Number of movements requested per page from the backend
const PAGE_SIZE = 10;

@Component({
  selector: 'app-current-account',
  standalone: true,
  imports: [CommonModule, FormsModule, MonthlyEvolutionChartComponent],
  templateUrl: './current-account.html',
  styleUrl: './current-account.scss'
})
export class CurrentAccountComponent implements OnInit, OnDestroy {
  // Movements for the page currently being displayed, already filtered/sorted by the backend
  public pagedMovements: MovementDto[] = [];

  // Bumped whenever the movements change so the evolution chart drops any history it cached
  // for its per-day breakdown. The chart loads that history itself, and only when a month is
  // actually selected, so this page never pulls the full movement list up front.
  public chartDataVersion = 0;

  // --- Date range filter ---
  public dateFilterMode: DateFilterMode = 'transactionDate';
  public dateFrom = '';
  public dateTo = '';

  // Current account balance (from the most recent movement, independent of filters)
  public currentBalance: number | null = null;

  // Sum of credit ("C") / debit ("D") movements matching the active filter
  public totalCredits = 0;
  public totalDebits = 0;

  // True while the initial movements + totals request is in flight
  public isLoading = true;

  // True while a page/filter change is being fetched (keeps old rows visible underneath)
  public isPageLoading = false;

  // Populated with a user-facing message when a movements/totals request fails
  public errorMessage: string | null = null;

  // Tracks which movement row currently has its note field open for editing
  public editingMovementId: string | null = null;

  // Working copy of the note text for the row currently being edited
  public noteDraft = '';

  // True while a note update request is in flight
  public isSavingNote = false;

  // --- Pagination (server-side) ---
  public currentPage = 1;
  public totalPages = 1;
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
    this.isLoading = true;
    this.errorMessage = null;

    // Load the first table page and the summary totals in parallel
    this.loadMovements();
    this.loadTotals();

    // Listen for live "documentProcessed" pushes so a statement import submitted from this
    // page reflects success/failure without the user needing to refresh
    this.documentHub.connect();
    this.hubSubscription = this.documentHub.onDocumentProcessed().subscribe((item) => this.onDocumentProcessed(item));
  }

  public ngOnDestroy(): void {
    this.hubSubscription?.unsubscribe();
    this.documentHub.disconnect();
  }

  // Build the query params for the active date-range filter, applied to whichever
  // date field (transaction or value date) the user picked
  private buildQueryParams(page: number): MovementQueryParams {
    const params: MovementQueryParams = { page, pageSize: this.pageSize };

    if (this.dateFrom || this.dateTo) {
      const field: DateFilterField = this.dateFilterMode;
      if (field === 'transactionDate') {
        if (this.dateFrom) params.transactionDateFrom = this.dateFrom;
        if (this.dateTo) params.transactionDateTo = this.dateTo;
      } else {
        if (this.dateFrom) params.valueDateFrom = this.dateFrom;
        if (this.dateTo) params.valueDateTo = this.dateTo;
      }
    }

    return params;
  }

  // Fetch the current table page from the backend (already filtered/sorted/paginated)
  public loadMovements(): void {
    this.isPageLoading = true;
    this.errorMessage = null;

    this.accountService.getMovements(this.buildQueryParams(this.currentPage)).subscribe({
      next: (response) => {
        this.pagedMovements = response.items;
        this.totalPages = Math.max(1, response.totalPages);
        this.currentPage = response.page;
        this.isPageLoading = false;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Não foi possível carregar os movimentos da conta. Tente novamente mais tarde.';
        this.isPageLoading = false;
        this.isLoading = false;
      }
    });
  }

  // Fetch the summary totals (credits/debits for the active filter, plus the current balance)
  public loadTotals(): void {
    this.accountService.getMovementTotals(this.buildQueryParams(1)).subscribe({
      next: (totals) => {
        this.totalCredits = totals.totalCredits;
        this.totalDebits = totals.totalDebits;
        this.currentBalance = totals.currentBalance;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Não foi possível carregar os totais da conta. Tente novamente mais tarde.';
      }
    });
  }

  // Reload the table and totals for the current filter selection (resets to page 1)
  public applyDateFilter(): void {
    this.currentPage = 1;
    this.loadMovements();
    this.loadTotals();
  }

  // Clear the active date filter and reload
  public clearDateFilter(): void {
    this.dateFrom = '';
    this.dateTo = '';
    this.applyDateFilter();
  }

  // --- Pagination helpers (server-side: each page change triggers a new request) ---

  public goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) {
      return;
    }
    this.currentPage = page;
    this.loadMovements();
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
      next: (updatedMovement) => {
        // Sync the row from the backend's response, rather than assuming the request succeeded silently
        movement.userNote = updatedMovement.userNote;
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
      // Refresh the table and totals so the newly imported statement is reflected everywhere
      this.currentPage = 1;
      this.loadMovements();
      this.loadTotals();

      // Signal the chart that the movements changed, so it drops any history it cached
      this.chartDataVersion++;
      this.cdr.detectChanges();
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
