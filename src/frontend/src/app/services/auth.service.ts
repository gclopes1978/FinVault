// Import the core Injectable decorator from the Angular framework
import { Injectable } from '@angular/core';
// Import the HTTP client used to communicate with the Identity microservice through the gateway
import { HttpClient } from '@angular/common/http';
// Import the Observable and tap operator to react to the login response as a side effect
import { Observable, tap } from 'rxjs';
// Import the environment configuration to resolve the API Gateway base address
import { environment } from '../../environments/environment';

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

  // Submit the user credentials to the Identity service and persist the returned token on success
  login(request: LoginRequest): Observable<AuthenticationResponse> {
    return this.http
      .post<AuthenticationResponse>(`${environment.gatewayUrl}/v1/auth/login`, request)
      .pipe(
        tap((response) => this.setToken(response.accessToken))
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
    return !!this.getToken();
  }

  // Persist the bearer token in sessionStorage so it is cleared automatically when the tab closes
  private setToken(token: string): void {
    sessionStorage.setItem(TOKEN_STORAGE_KEY, token);
  }
}
