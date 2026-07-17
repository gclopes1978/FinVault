// Import the functional guard contract and the Router to redirect unauthenticated users
import { CanActivateFn, Router } from '@angular/router';
// Import the dependency injection helper used to resolve services inside a functional guard
import { inject } from '@angular/core';
// Import the AuthService to check whether an active session exists
import { AuthService } from '../services/auth.service';

/**
 * Route guard that blocks navigation to protected views when no valid session token exists,
 * redirecting the user to the Login page instead.
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // No active session: send the user back to the login page
  return router.parseUrl('/login');
};
