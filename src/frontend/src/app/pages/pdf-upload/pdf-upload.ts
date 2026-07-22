// Import the core Component decorator from the Angular framework
import { Component } from '@angular/core';
// Import CommonModule for structural directives and pipes (date, currency, etc.)
import { CommonModule } from '@angular/common';
// Import FormsModule to support the two-way bound document type selector
import { FormsModule } from '@angular/forms';
// Import the HTTP primitives used to interpret upload progress and error responses
import { HttpErrorResponse, HttpEventType } from '@angular/common/http';
// Import the upload service and its supporting types
import { PdfDocumentType, PdfUploadResult, PdfUploadService } from '../../services/pdf-upload.service';

// Describe a selectable entry in the document type dropdown/selector
interface DocumentTypeOption {
  value: PdfDocumentType;
  label: string;
}

// Track which visual state the upload area is currently showing
type UploadStage = 'idle' | 'uploading' | 'parsing' | 'success' | 'error';

// Describe a single row of the import history table
interface ImportHistoryEntry {
  date: Date;
  documentTypeLabel: string;
  status: 'Sucesso' | 'Erro' | 'Enviado';
  recordsImported: number | null;
}

// Cap the import history table to the last 20 uploads, per the Definition of Done
const MAX_HISTORY_ENTRIES = 20;

@Component({
  selector: 'app-pdf-upload',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pdf-upload.html',
  styleUrl: './pdf-upload.scss'
})
export class PdfUploadComponent {
  // The four PDF document types currently supported for manual import
  public documentTypes: DocumentTypeOption[] = [
    { value: 'ActivoBankStatement', label: 'Extrato ActivoBank (histórico)' },
    { value: 'PayrollSlip', label: 'Recibo de Vencimento' },
    { value: 'LusitaniaPPR', label: 'PPR Lusitânia Vida' },
    { value: 'ARBancoInvestPPR', label: 'PPR AR Banco Invest' }
  ];

  public selectedDocumentType: PdfDocumentType = this.documentTypes[0].value;
  public isDragging = false;
  public stage: UploadStage = 'idle';
  public uploadProgress = 0;
  public errorMessage: string | null = null;
  public lastResult: PdfUploadResult | null = null;
  public importHistory: ImportHistoryEntry[] = [];

  constructor(private pdfUploadService: PdfUploadService) {}

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

    const documentTypeLabel =
      this.documentTypes.find((type) => type.value === this.selectedDocumentType)?.label ??
      this.selectedDocumentType;

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
          this.addHistoryEntry(documentTypeLabel, 'Enviado', this.lastResult?.recordsImported ?? null);
        }
      },
      error: (err: HttpErrorResponse) => {
        this.stage = 'error';
        this.errorMessage = this.extractErrorMessage(err);
        this.addHistoryEntry(documentTypeLabel, 'Erro', null);
      }
    });
  }

  private extractErrorMessage(err: HttpErrorResponse): string {
    const detail = err.error?.detail ?? err.error?.message ?? err.error;
    if (typeof detail === 'string' && detail.trim().length > 0) {
      return detail;
    }
    return 'Documento não reconhecido ou erro ao processar o PDF.';
  }

  private addHistoryEntry(
    documentTypeLabel: string,
    status: ImportHistoryEntry['status'],
    recordsImported: number | null
  ): void {
    this.importHistory.unshift({
      date: new Date(),
      documentTypeLabel,
      status,
      recordsImported
    });

    if (this.importHistory.length > MAX_HISTORY_ENTRIES) {
      this.importHistory.length = MAX_HISTORY_ENTRIES;
    }
  }
}
