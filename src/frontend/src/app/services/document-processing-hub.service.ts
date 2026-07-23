// Import the core Injectable decorator from the Angular framework
import { Injectable } from '@angular/core';
// Import the SignalR client used to receive live push events from DocumentIngestion
import * as signalR from '@microsoft/signalr';
// Import Observable/Subject to expose the incoming events as a stream
import { Observable, Subject } from 'rxjs';
// Import the environment configuration to resolve the API Gateway base address
import { environment } from '../../environments/environment';
// Import the shape of the payload the hub sends
import { UploadHistoryItem } from './pdf-upload.service';

@Injectable({
  providedIn: 'root'
})
export class DocumentProcessingHubService {
  private hubConnection: signalR.HubConnection | null = null;
  private readonly documentProcessed = new Subject<UploadHistoryItem>();

  // Emits every time DocumentIngestion reports a document as finished processing
  // (success or error), pushed live by the "documentProcessed" hub event.
  public onDocumentProcessed(): Observable<UploadHistoryItem> {
    return this.documentProcessed.asObservable();
  }

  // Open the SignalR connection. Safe to call multiple times — a no-op once already connected.
  public connect(): void {
    if (this.hubConnection) {
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      // Reached through the Gateway's existing document-ingestion route/transform
      // (/api/v1/document/{**catch-all} -> /api/document/{**catch-all}), so no extra
      // Gateway configuration is needed beyond what /v1/document/upload already uses.
      .withUrl(`${environment.gatewayUrl}/v1/document/hub`, { withCredentials: false })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('documentProcessed', (item: UploadHistoryItem) => {
      this.documentProcessed.next(item);
    });

    this.hubConnection.start().catch((error) => {
      console.error('[DocumentProcessingHub] Connection failed:', error);
    });
  }

  // Close the SignalR connection (e.g. when leaving the PDF Upload Centre page).
  public disconnect(): void {
    this.hubConnection?.stop();
    this.hubConnection = null;
  }
}
