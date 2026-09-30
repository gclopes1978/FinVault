import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { MovementDto, MovementTotalsByYearMonth, MovementQueryParams, MovementTotals, PagedMovementsResponse } from '../models/movement';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  // Base URL for the Current Account module, proxied by the gateway's currentaccount-route
  // (gateway rewrites /api/v1/accounts/** to the backend's /api/movements/**)
  private readonly apiUrl = `${environment.gatewayUrl}/v1/accounts`;

  constructor(private readonly http: HttpClient) {}

  // Fetch a single page of movements for the authenticated user's current account, filtered by
  // date range and sorted server-side (TransactionDate desc, ValueDate desc by default)
  public getMovements(params: MovementQueryParams): Observable<PagedMovementsResponse> {
    return this.http.get<PagedMovementsResponse>(this.apiUrl, { params: this.buildHttpParams(params) });
  }

  // Fetch aggregate credit/debit totals (respecting the same date filters) plus the current balance
  public getMovementTotals(params: MovementQueryParams): Observable<MovementTotals> {
    return this.http.get<MovementTotals>(`${this.apiUrl}/totals`, { params: this.buildHttpParams(params) });
  }

  public getAvailableYears(): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiUrl}/years`);
  }

  public getTotalsByYearMonth(year: number): Observable<MovementTotalsByYearMonth[]> {
    const params = new HttpParams()
      .set('year', year);

    return this.http.get<MovementTotalsByYearMonth[]>(
      `${this.apiUrl}/totals/year`,
      { params }
    );
  }
  // Update the personal note attached to a single movement; the backend returns the updated movement
  public updateMovementNote(movementId: string, note: string): Observable<MovementDto> {
    return this.http.patch<MovementDto>(`${this.apiUrl}/${movementId}/note`, { note });
  }


  private buildHttpParams(params: MovementQueryParams): HttpParams {
    let httpParams = new HttpParams();

    if (params.page != null) httpParams = httpParams.set('page', params.page);
    if (params.pageSize != null) httpParams = httpParams.set('pageSize', params.pageSize);
    if (params.transactionDateFrom) httpParams = httpParams.set('transactionDateFrom', params.transactionDateFrom);
    if (params.transactionDateTo) httpParams = httpParams.set('transactionDateTo', params.transactionDateTo);
    if (params.valueDateFrom) httpParams = httpParams.set('valueDateFrom', params.valueDateFrom);
    if (params.valueDateTo) httpParams = httpParams.set('valueDateTo', params.valueDateTo);

    return httpParams;
  }
}
