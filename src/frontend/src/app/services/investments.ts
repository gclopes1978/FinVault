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

  importTrading212Csv(file: File): Observable<{ importedRecords: number }> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<{ importedRecords: number }>(
      `${this.apiUrl}/import/trading212`,
      formData
    );
  }

  getMarketPrice(assetType: string, symbol: string): Observable<{
    symbol: string;
    price: number;
    currency: string;
    timestamp: string;
  }> {
    return this.http.get<{
      symbol: string;
      price: number;
      currency: string;
      timestamp: string;
    }>(
      `${environment.gatewayUrl}/v1/Market-Prices/${assetType}/${symbol}`
    );
  }

  getExchangeRate(fromCurrency: string, toCurrency: string): Observable<number> {
    return this.http.get<number>(
      `${environment.gatewayUrl}/v1/market-prices/exchange-rate/${fromCurrency}/${toCurrency}`
    );
  }
}