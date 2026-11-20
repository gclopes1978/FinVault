import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

import {
  PensionFundMovement,
  PensionInsuranceMovement
} from '../models/pension-plan';

@Injectable({
  providedIn: 'root'
})
export class PensionPlansService {
  private readonly baseUrl = `${environment.gatewayUrl}/v1/pension-plans`;

  constructor(private readonly http: HttpClient) {}

  getFundMovements(): Observable<PensionFundMovement[]> {
    return this.http.get<PensionFundMovement[]>(
      `${this.baseUrl}/movements/funds`
    );
  }

  getInsuranceMovements(): Observable<PensionInsuranceMovement[]> {
    return this.http.get<PensionInsuranceMovement[]>(
      `${this.baseUrl}/movements/insurance`
    );
  }
}