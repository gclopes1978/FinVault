// Import Component/OnInit/OnDestroy from the Angular framework
import { Component, OnDestroy, OnInit } from '@angular/core';
// Import CommonModule for structural directives and pipes (date, currency, etc.)
import { CommonModule } from '@angular/common';
// Import FormsModule to support the two-way bound document type selector
import { FormsModule } from '@angular/forms';
// Import the HTTP primitives used to interpret upload progress and error responses
import { HttpErrorResponse, HttpEventType } from '@angular/common/http';
// Import the subscription type used to stop the post-upload history polling
import { Subscription, interval } from 'rxjs';
// Import the upload service and its supporting types
import { PdfDocumentType, PdfUploadResult, PdfUploadService, UploadHistoryItem } from '../../services/pdf-upload.service';

// Describe a selectable entry in the document type dropdown/selector
interface DocumentTypeOption {
  value: PdfDocumentType;
  label: string;
}

// Track which visual state the upload area is currently showing
type UploadStage = 'idle' | 'uploading' | 'parsing' | 'success' | 'error';

// How often (ms) to refresh the history table while a just-uploaded document is still pending,
// so its status updates automatically once the PdfParser reports back
const HISTORY_POLL_INTERVAL_MS = 2500;
// Stop polling after this many attempts even if the document never reports back, so the app
// doesn't poll forever if the PdfParser callback is misconfigured
const HISTORY_POLL_MAX_ATTEMPTS = 12;

@Component({
  selector: 'app-pdf-upload',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pdf-upload.html',
  styleUrl: './pdf-upload.scss'
})
export class PdfUploadComponent implements OnInit, OnDestroy {
  // The four PDF document types currently supported for manual import
  public documentTypes: DocumentTypeOption[] = [
    { value: 'ActivoBankStatement', label: 'Extrato ActivoBank (histórico)' },
    { value: 'PayrollSlip', label: 'Recibo de Vencimento' },
    { value: 'LusitaniaPPR', label: 'PPR Lusitânia Vida' },
    { value: 'ARBancoInvestPPR', label: 'PPR AR Banco Invest' }
  ];

  // Maps both the frontend type hints and the PdfParser's own classifier labels to display text,
  // so the history table reads correctly whichever one ended up stored (see UploadedDocument.DocumentType).
  private readonly documentTypeLabels: Record<string, string> = {
    ActivoBankStatement: 'Extrato ActivoBank (histórico)',
    PayrollSlip: 'Recibo de Vencimento',
    LusitaniaPPR: 'PPR Lusitânia Vida',
    ARBancoInvestPPR: 'PPR AR Banco Invest',
    bank_statement: 'Extrato Bancário',
    payroll: 'Recibo de Vencimento',
    pension_insurance: 'PPR Lusitânia Vida',
    pension_fund: 'PPR AR Banco Invest'
  };

  public selectedDocumentType: PdfDocumentType = this.documentTypes[0].value;
  public isDragging = false;
  public stage: UploadStage = 'idle';
  public uploadProgress = 0;
  public errorMessage: string | null = null;
  public lastResult: PdfUploadResult | null = null;

  public importHistory: UploadHistoryItem[] = [];
  public isHistoryLoading = false;

  private historyPollSubscription: Subscription | null = null;

  constructor(private pdfUploadService: PdfUploadService) {}

  public ngOnInit(): void {
    this.loadHistory();
  }

  public ngOnDestroy(): void {
    this.historyPollSubscription?.unsubscribe();
  }

  // Whether user interaction with the upload area should currently be blocked
  public get isBusy(): boolean {
    return this.stage === 'uploading' || this.stage === 'parsing';
  }

  public onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (!this.isBusy) {
      this.isDragging = true;
    }
  }

  public onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
  }

  public onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;

    if (this.isBusy) {
      return;
    }

    const droppedFile = event.dataTransfer?.files?.[0];
    if (droppedFile) {
      this.handleFile(droppedFile);
    }
  }

  public onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const selectedFile = input.files?.[0];
    if (selectedFile) {
      this.handleFile(selectedFile);
    }
    // Reset the input so selecting the same file twice in a row still fires a change event
    input.value = '';
  }

  // Return to the idle state so the user can upload another document
  public resetUpload(): void {
    this.stage = 'idle';
    this.uploadProgress = 0;
    this.errorMessage = null;
    this.lastResult = null;
  }

  // Resolve a document type value (either a frontend hint or a PdfParser classifier label) into
  // a friendly Portuguese label, falling back to the raw value for anything unrecognised.
  public documentTypeLabel(documentType: string | null): string {
    if (!documentType) {
      return '—';
    }
    return this.documentTypeLabels[documentType] ?? documentType;
  }

  // Render a human-friendly status for a history row: pending while Success is still null.
  public historyStatusLabel(item: UploadHistoryItem): string {
    if (item.success === null) {
      return 'Em processamento';
    }
    return item.success ? 'Sucesso' : 'Erro';
  }

  public historyStatusClass(item: UploadHistoryItem): Record<string, boolean> {
    return {
      'badge-success': item.success === true,
      'badge-error': item.success === false,
      'badge-pending': item.success === null
    };
  }

  // Format a byte count as a short, human-readable size (KB/MB)
  public formatFileSize(bytes: number): string {
    if (bytes < 1024) {
      return `${bytes} B`;
    }
    if (bytes < 1024 * 1024) {
      return `${(bytes / 1024).toFixed(1)} KB`;
    }
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  private loadHistory(): void {
    this.isHistoryLoading = true;
    this.pdfUploadService.getHistory(20).subscribe({
      next: (history) => {
        this.importHistory = history;
        this.isHistoryLoading = false;
      },
      error: () => {
        this.isHistoryLoading = false;
      }
    });
  }

  private handleFile(file: File): void {
    const isPdf = file.type === 'application/pdf' || file.name.toLowerCase().endsWith('.pdf');
    if (!isPdf) {
      this.stage = 'error';
      this.errorMessage = 'Documento não reconhecido: apenas são aceites ficheiros PDF.';
      return;
    }

    this.stage = 'uploading';
    this.uploadProgress = 0;
    this.errorMessage = null;
    this.lastResult = null;

    this.pdfUploadService.uploadPdf(file, this.selectedDocumentType).subscribe({
      next: (event) => {
        if (event.type === HttpEventType.UploadProgress && event.total) {
          this.uploadProgress = Math.round((100 * event.loaded) / event.total);
          // Once the bytes are fully sent, the backend is still parsing/dispatching the document
          if (this.uploadProgress >= 100) {
            this.stage = 'parsing';
          }
        } else if (event.type === HttpEventType.Response) {
          this.stage = 'success';
          this.lastResult = event.body ?? null;
          this.loadHistory();
          if (this.lastResult?.documentId) {
            this.pollHistoryUntilProcessed(this.lastResult.documentId);
          }
        }
      },
      error: (err: HttpErrorResponse) => {
        this.stage = 'error';
        this.errorMessage = this.extractErrorMessage(err);
        this.loadHistory();
      }
    });
  }

  // The upload response only confirms the document was received — actual PDF processing happens
  // asynchronously via RabbitMQ/PdfParser. Poll the history briefly afterwards so the "Em
  // processamento" row updates to "Sucesso"/"Erro" without the user needing to refresh manually.
  private pollHistoryUntilProcessed(documentId: string): void {
    this.historyPollSubscription?.unsubscribe();

    let attempts = 0;
    this.historyPollSubscription = interval(HISTORY_POLL_INTERVAL_MS).subscribe(() => {
      attempts++;
      this.pdfUploadService.getHistory(20).subscribe({
        next: (history) => {
          this.importHistory = history;
          const uploaded = history.find((item) => item.id === documentId);
          if ((uploaded && uploaded.success !== null) || attempts >= HISTORY_POLL_MAX_ATTEMPTS) {
            this.historyPollSubscription?.unsubscribe();
          }
        },
        error: () => {
          this.historyPollSubscription?.unsubscribe();
        }
      });
    });
  }

  private extractErrorMessage(err: HttpErrorResponse): string {
    const detail = err.error?.detail ?? err.error?.message ?? err.error;
    if (typeof detail === 'string' && detail.trim().length > 0) {
      return detail;
    }
    return 'Documento não reconhecido ou erro ao processar o PDF.';
  }
}
