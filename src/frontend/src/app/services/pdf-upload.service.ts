// Import the core Injectable decorator from the Angular framework
import { Injectable } from '@angular/core';
// Import the HTTP client primitives needed to track upload progress via low-level HttpRequest
import { HttpClient, HttpEvent, HttpRequest } from '@angular/common/http';
// Import the Observable type used to stream upload progress and the final response
import { Observable } from 'rxjs';
// Import the environment configuration to resolve the API Gateway base address
import { environment } from '../../environments/environment';

// Define the four document types currently supported by the PDF Upload Centre
export type PdfDocumentType =
  | 'ActivoBankStatement'
  | 'PayrollSlip'
  | 'LusitaniaPPR'
  | 'ARBancoInvestPPR';

// Define a type-safe contract for the response returned once the document has been accepted.
// NOTE: the DocumentIngestion service (api/v1/document/upload) returns message/documentId/path
// synchronously — actual parsing happens asynchronously via the PdfParser and is reflected in
// the import history (see UploadHistoryItem below). The optional period/recordsImported/
// totalValue fields are kept here so the summary card can display richer data automatically if
// the upload endpoint is ever extended to return it synchronously.
export interface PdfUploadResult {
  documentId?: string;
  message?: string;
  period?: string;
  recordsImported?: number;
  totalValue?: number;
}

// A single row of the import history, as persisted by DocumentIngestion and updated once the
// PdfParser reports back whether processing succeeded. `success` is null while still pending.
export interface UploadHistoryItem {
  id: string;
  fileName: string;
  documentType: string | null;
  fileSizeBytes: number;
  uploadedAt: string;
  processedAt: string | null;
  success: boolean | null;
  errorMessage: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class PdfUploadService {
  constructor(private http: HttpClient) {}

  // Upload a PDF file together with the selected document type hint, streaming progress events
  // so the UI can render a live progress bar followed by a "parsing" state.
  uploadPdf(file: File, documentType: PdfDocumentType): Observable<HttpEvent<PdfUploadResult>> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    formData.append('documentType', documentType);

    const request = new HttpRequest<FormData>(
      'POST',
      `${environment.gatewayUrl}/v1/document/upload`,
      formData,
      { reportProgress: true }
    );

    return this.http.request<PdfUploadResult>(request);
  }

  // Fetch the most recent uploads (default: last 20) to populate the import history table.
  getHistory(take: number = 20): Observable<UploadHistoryItem[]> {
    return this.http.get<UploadHistoryItem[]>(`${environment.gatewayUrl}/v1/document/history`, {
      params: { take }
    });
  }
}
