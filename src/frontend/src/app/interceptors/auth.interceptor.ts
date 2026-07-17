// Import the functional interceptor contract from the Angular HTTP client module
import { HttpInterceptorFn } from '@angular/common/http';
// Import the dependency injection helper used to resolve services inside a functional interceptor
import { inject } from '@angular/core';
// Import the AuthService to read the currently persisted bearer token
import { AuthService } from '../services/auth.service';

/**
 * Global functional interceptor that clones outgoing HTTP requests and appends
 * the "Authorization: Bearer <token>" header whenever a token exists in sessionStorage.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  // Leave the request untouched when there is no active session
  if (!token) {
    return next(req);
  }

  // Clone the request and inject the Authorization header instead of mutating the original
  const authorizedRequest = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authorizedRequest);
};
