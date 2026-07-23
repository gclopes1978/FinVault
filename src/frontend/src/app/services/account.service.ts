import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { MovementDto } from '../models/movement';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  // Base URL for the Current Account module, proxied by the gateway's currentaccount-route
  // (gateway rewrites /api/v1/accounts/** to the backend's /api/movements/**)
  private readonly apiUrl = `${environment.gatewayUrl}/v1/accounts`;

  constructor(private readonly http: HttpClient) {}

  // Fetch every recorded movement for the authenticated user's current account
  public getMovements(): Observable<MovementDto[]> {
    return this.http.get<MovementDto[]>(this.apiUrl);
  }

  // Update the personal note attached to a single movement
  public updateMovementNote(movementId: string, note: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${movementId}/note`, { note });
  }
}
