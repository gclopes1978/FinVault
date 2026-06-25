import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { MovementDto } from '../models/movement';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  // Compute and preserve the exact specific base URL route targeting account subsystem resources
  private readonly apiUrl = `${environment.gatewayUrl}/accounts`;

  // Inject the type-safe internal Angular HttpClient engine dependency module instance parameter
  constructor(private readonly http: HttpClient) {}

  // Request the complete structural tracking history collection payload from database endpoints
  public getStatement(accountId: string): Observable<MovementDto[]> {
    // Dispatch a non-blocking asynchronous GET network request contract action over to the backend proxy
    return this.http.get<MovementDto[]>(`${this.apiUrl}/${accountId}/statement`);
  }

  // Submit a newly generated structural deposit or withdraw operational transaction record into live pipelines
  public createMovement(accountId: string, movement: Partial<MovementDto>): Observable<MovementDto> {
    // Dispatch an asynchronous HTTP POST mutation tracking transaction payload sequence targeting gateway addresses
    return this.http.post<MovementDto>(`${this.apiUrl}/${accountId}/movements`, movement);
  }
}