import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Portfolio } from '../models/portfolio';
import { Transaction } from '../models/transaction';

@Injectable({
  providedIn: 'root'
})
export class InvestmentsService {
  private readonly apiUrl = `${environment.gatewayUrl}/v1/investments`;

  constructor(private readonly http: HttpClient) {}

  getPortfolio(): Observable<Portfolio[]> {
    return this.http.get<Portfolio[]>(`${this.apiUrl}/portfolio`);
  }

  getPortfolioHistory(): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/portfolio/history`);
  }

  importXtbCsv(file: File): Observable<{ importedRecords: number }> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<{ importedRecords: number }>(
      `${this.apiUrl}/import/xtb`,
      formData
    );
  }
}