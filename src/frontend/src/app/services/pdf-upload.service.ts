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
// NOTE: the DocumentIngestion service (api/v1/document/upload) currently only returns
// message/documentId/path — it does not yet parse the PDF synchronously. The optional
// period/recordsImported/totalValue fields are kept here so the UI can display a richer
// summary automatically once the backend contract is extended to return them.
export interface PdfUploadResult {
  documentId?: string;
  message?: string;
  period?: string;
  recordsImported?: number;
  totalValue?: number;
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
    // Sent for forward-compatibility: the current DocumentIngestion endpoint does not yet bind
    // this field, but the PDF Upload Centre already passes the type hint the API is expected to use.
    formData.append('documentType', documentType);

    const request = new HttpRequest<FormData>(
      'POST',
      `${environment.gatewayUrl}/v1/document/upload`,
      formData,
      { reportProgress: true }
    );

    return this.http.request<PdfUploadResult>(request);
  }
}
