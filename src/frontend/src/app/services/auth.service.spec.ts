import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    sessionStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should store the access token in sessionStorage on successful login', () => {
    const mockResponse = { accessToken: 'fake-jwt-token', expiration: '2026-01-01T00:00:00Z' };

    service.login({ email: 'user@example.com', password: 'secret123' }).subscribe();

    const req = httpMock.expectOne(`${environment.gatewayUrl}/v1/auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);

    expect(service.getToken()).toBe('fake-jwt-token');
    expect(service.isAuthenticated()).toBe(true);
  });

  it('should clear the token on logout', () => {
    sessionStorage.setItem('finvault_access_token', 'fake-jwt-token');

    service.logout();

    expect(service.getToken()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
  });

  it('should report unauthenticated when no token is present', () => {
    expect(service.isAuthenticated()).toBe(false);
  });
});
