import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';

import { environment } from '../../environments/environment';
import { PensionPlansService } from './pension-plans';

describe('PensionPlansService', () => {
  let service: PensionPlansService;
  let httpTestingController: HttpTestingController;

  const baseUrl = `${environment.gatewayUrl}/v1/pension-plans`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(PensionPlansService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should request pension fund movements', () => {
    const movements = [
      {
        id: '1',
        date: '2026-10-01',
        quantity: 10,
        unitPrice: 12.5,
        valuation: 125
      }
    ];

    service.getFundMovements().subscribe((result) => {
      expect(result).toEqual(movements);
    });

    const request = httpTestingController.expectOne(
      `${baseUrl}/movements/funds`
    );

    expect(request.request.method).toBe('GET');
    request.flush(movements);
  });

  it('should request pension insurance movements', () => {
    const movements = [
      {
        movement: 'Contribution',
        receipt: 'R-001',
        date: '2026-10-01',
        debit: null,
        credit: 100,
        balance: 100
      }
    ];

    service.getInsuranceMovements().subscribe((result) => {
      expect(result).toEqual(movements);
    });

    const request = httpTestingController.expectOne(
      `${baseUrl}/movements/insurance`
    );

    expect(request.request.method).toBe('GET');
    request.flush(movements);
  });
});