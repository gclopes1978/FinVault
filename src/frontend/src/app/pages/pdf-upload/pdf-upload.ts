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
import { PdfDocumentType, PdfUploadService, UploadHistoryItem } from '../../services/pdf-upload.service';

// Describe a selectable entry in the document type dropdown/selector
interface DocumentTypeOption {
  value: PdfDocumentType;
  label: string;
}

// Track which visual state a single queued upload is currently showing
type UploadTaskStage = 'uploading' | 'parsing' | 'success' | 'error';

// One row in the "current batch" list — one per file selected/dropped in a single interaction.
// Files are uploaded one at a time (see processQueue) since the PdfParser has a single
// RabbitMQ consumer and processes documents sequentially regardless of how fast we upload them.
interface UploadTask {
  id: string;
  fileName: string;
  stage: UploadTaskStage;
  progress: number;
  errorMessage: string | null;
  documentId: string | null;
}

// How often (ms) to refresh the history table while just-uploaded documents are still pending,
// so their status updates automatically once the PdfParser reports back
const HISTORY_POLL_INTERVAL_MS = 2500;
// Stop polling after this many attempts even if a document never reports back, so the app
// doesn't poll forever if the PdfParser callback is misconfigured
const HISTORY_POLL_MAX_ATTEMPTS = 20;

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

  // The current batch: every file selected/dropped together, with its own live progress/status
  public uploadTasks: UploadTask[] = [];

  public importHistory: UploadHistoryItem[] = [];
  public isHistoryLoading = false;

  private uploadQueue: { file: File; task: UploadTask }[] = [];
  private isProcessingQueue = false;
  private historyPollSubscription: Subscription | null = null;

  constructor(private pdfUploadService: PdfUploadService) {}

  public ngOnInit(): void {
    this.loadHistory();
  }

  public ngOnDestroy(): void {
    this.historyPollSubscription?.unsubscribe();
  }

  // Whether user interaction with the drop zone should currently be blocked
  public get isBusy(): boolean {
    return this.isProcessingQueue;
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

    const droppedFiles = event.dataTransfer?.files;
    if (droppedFiles && droppedFiles.length > 0) {
      this.handleFiles(Array.from(droppedFiles));
    }
  }

  public onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFiles(Array.from(input.files));
    }
    // Reset the input so selecting the same file(s) twice in a row still fires a change event
    input.value = '';
  }

  // Clear the current batch list so the drop zone is free for another round of uploads
  public clearCompletedTasks(): void {
    this.uploadTasks = [];
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

  private handleFiles(files: File[]): void {
    for (const file of files) {
      const isPdf = file.type === 'application/pdf' || file.name.toLowerCase().endsWith('.pdf');
      const task: UploadTask = {
        id: `${Date.now()}-${Math.random().toString(36).slice(2)}`,
        fileName: file.name,
        stage: isPdf ? 'uploading' : 'error',
        progress: 0,
        errorMessage: isPdf ? null : 'Documento não reconhecido: apenas são aceites ficheiros PDF.',
        documentId: null
      };

      this.uploadTasks.unshift(task);

      if (isPdf) {
        this.uploadQueue.push({ file, task });
      }
    }

    if (!this.isProcessingQueue) {
      this.processQueue();
    }
  }

  // Uploads are processed one file at a time, in the order they were added. This matches the
  // PdfParser's single RabbitMQ consumer (it processes one document at a time regardless of how
  // many arrive at once), and avoids saturating the Gateway with dozens of simultaneous uploads.
  private processQueue(): void {
    const next = this.uploadQueue.shift();

    if (!next) {
      this.isProcessingQueue = false;
      this.onBatchDrained();
      return;
    }

    this.isProcessingQueue = true;
    this.uploadOne(next.file, next.task).then(() => this.processQueue());
  }

  private uploadOne(file: File, task: UploadTask): Promise<void> {
    return new Promise((resolve) => {
      this.pdfUploadService.uploadPdf(file, this.selectedDocumentType).subscribe({
        next: (event) => {
          if (event.type === HttpEventType.UploadProgress && event.total) {
            task.progress = Math.round((100 * event.loaded) / event.total);
            // Once the bytes are fully sent, the backend is still parsing/dispatching the document
            if (task.progress >= 100) {
              task.stage = 'parsing';
            }
          } else if (event.type === HttpEventType.Response) {
            task.stage = 'success';
            task.documentId = event.body?.documentId ?? null;
            resolve();
          }
        },
        error: (err: HttpErrorResponse) => {
          task.stage = 'error';
          task.errorMessage = this.extractErrorMessage(err);
          resolve();
        }
      });
    });
  }

  // Once every file in the batch has been sent, refresh the history table and poll it briefly so
  // the newly uploaded rows move from "Em processamento" to "Sucesso"/"Erro" automatically.
  private onBatchDrained(): void {
    this.loadHistory();

    const pendingDocumentIds = this.uploadTasks
      .filter((task) => task.stage === 'success' && task.documentId)
      .map((task) => task.documentId as string);

    this.pollHistoryUntilProcessed(pendingDocumentIds);
  }

  private pollHistoryUntilProcessed(documentIds: string[]): void {
    this.historyPollSubscription?.unsubscribe();

    if (documentIds.length === 0) {
      return;
    }

    let attempts = 0;
    this.historyPollSubscription = interval(HISTORY_POLL_INTERVAL_MS).subscribe(() => {
      attempts++;
      this.pdfUploadService.getHistory(20).subscribe({
        next: (history) => {
          this.importHistory = history;

          const stillPending = documentIds.some((id) => {
            const item = history.find((h) => h.id === id);
            return !item || item.success === null;
          });

          if (!stillPending || attempts >= HISTORY_POLL_MAX_ATTEMPTS) {
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
