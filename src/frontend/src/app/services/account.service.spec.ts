import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AccountService } from './account.service';

describe('AccountService', () => {
  let service: AccountService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AccountService]
    });
    service = TestBed.inject(AccountService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch account balance', () => {
    const mockBalance = { balance: 1500.00 };
    
    service.getBalance().subscribe(data => {
      expect(data.balance).toBe(1500.00);
    });

    const req = httpMock.expectOne('/api/account/balance');
    expect(req.request.method).toBe('GET');
    req.flush(mockBalance);
  });
});
