import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  let authServiceStub: Partial<AuthService>;
  let router: Router;

  const runGuard = () =>
    TestBed.runInInjectionContext(() => authGuard(null as any, null as any));

  beforeEach(() => {
    authServiceStub = { isAuthenticated: () => false };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceStub },
        {
          provide: Router,
          useValue: { parseUrl: (url: string) => url }
        }
      ]
    });

    router = TestBed.inject(Router);
  });

  it('should allow activation when the user is authenticated', () => {
    authServiceStub.isAuthenticated = () => true;

    const result = runGuard();

    expect(result).toBe(true);
  });

  it('should redirect to /login when the user is not authenticated', () => {
    authServiceStub.isAuthenticated = () => false;

    const result = runGuard();

    expect(result).toBe('/login');
  });
});
