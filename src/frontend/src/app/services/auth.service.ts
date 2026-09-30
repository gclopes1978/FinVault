// Import the core Injectable decorator from the Angular framework
import { Injectable } from '@angular/core';
// Import the HTTP client and HttpErrorResponse used to communicate with the Identity microservice through the gateway
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
// Import the Observable and operators used to react to the login response as a side effect
import { Observable, map, switchMap, of, throwError, timeout } from 'rxjs';
// Import the environment configuration to resolve the API Gateway base address
import { environment } from '../../environments/environment';

interface JwtPayload {
  exp?: number;
  [claim: string]: unknown;
}

// Maximum time (ms) to wait for the Identity service to answer a login request
// before failing fast, so the UI never gets stuck on "Signing in..." indefinitely
const LOGIN_TIMEOUT_MS = 15000;



// Define the key used to persist the JWT bearer token inside sessionStorage
const TOKEN_STORAGE_KEY = 'finvault_access_token';

// Define a type-safe contract for the payload submitted to the login endpoint
export interface LoginRequest {
  email: string;
  password: string;
}

// Define a type-safe contract for the payload submitted to the register endpoint
export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

// Define a type-safe contract mapping the Identity service authentication response
export interface AuthenticationResponse {
  accessToken: string;
  expiration: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private http: HttpClient) {}

  // Submit the user credentials to the Identity service and persist the returned token on success.
  login(request: LoginRequest): Observable<AuthenticationResponse> {
    return this.http
      .post<any>(`${environment.gatewayUrl}/v1/auth/login`, request)
      .pipe(
        timeout(LOGIN_TIMEOUT_MS),
        switchMap((response: any) => {
          const token = response?.accessToken ?? response?.data?.accessToken ?? response?.token ?? response?.data?.token;

          if (token) {
            this.setToken(token);
            const expiration = response?.expiration ?? response?.expiresAt ?? response?.data?.expiration ?? response?.data?.expiresAt ?? undefined;
            return of({ accessToken: token, expiration } as AuthenticationResponse);
          }

          // If we got HTTP 200 but no token, raise a real Angular HttpErrorResponse
          // so LoginComponent's error handler can read it consistently
          const msg = response?.detail || response?.message || 'Authentication failed.';
          return throwError(() => new HttpErrorResponse({
            error: { detail: msg },
            status: 401,
            statusText: 'Unauthorized'
          }));
        })
      );
  }

  // Register a new user account through the Identity service
  register(request: RegisterRequest): Observable<{ message?: string; Message?: string }> {
    return this.http.post<{ message?: string; Message?: string }>(`${environment.gatewayUrl}/v1/auth/register`, request);
  }

  // Remove the persisted token, effectively logging the current user out
  logout(): void {
    sessionStorage.removeItem(TOKEN_STORAGE_KEY);
  }

  // Retrieve the currently stored bearer token, or null when no session exists
  getToken(): string | null {
    return sessionStorage.getItem(TOKEN_STORAGE_KEY);
  }

  // Determine whether the current session holds a token, used by route guards
  isAuthenticated(): boolean {
    const token = this.getToken();
    return !!token && !this.isTokenExpired(token);
  }

  public getTokenPayload(): JwtPayload | null {
    const token = this.getToken();
    return token ? this.decodeToken(token) : null;
  }

  public isTokenExpired(token?: string): boolean {
    const rawToken = token ?? this.getToken();
    if (!rawToken) {
      return true;
    }

    const payload = this.decodeToken(rawToken);
    if (!payload) {
      return false;
    }
    if (typeof payload.exp !== 'number') {
      return false;
    }

    return payload.exp * 1000 <= Date.now();
  }

  // Persist the bearer token in sessionStorage so it is cleared automatically when the tab closes
  private setToken(token: string): void {
    sessionStorage.setItem(TOKEN_STORAGE_KEY, token);
  }

  private decodeToken(token: string): JwtPayload | null {
    try {
      const payloadBase64 = token.split('.')[1]?.replace(/-/g, '+').replace(/_/g, '/');
      const paddedPayload = payloadBase64?.padEnd(payloadBase64.length + (4 - (payloadBase64.length % 4)) % 4, '=');
      return JSON.parse(atob(paddedPayload));
    } catch {
      return null;
    }
  }
}