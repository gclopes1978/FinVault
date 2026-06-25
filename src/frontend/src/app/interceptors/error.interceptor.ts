import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

/**
 * Global functional interceptor to handle HTTP error responses.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unknown error occurred!';

      // Handle specific HTTP status codes
      switch (error.status) {
        case 401:
          errorMessage = 'Unauthorized! Please log in again.';
          break;
        case 403:
          errorMessage = 'Forbidden! You do not have permission to access this resource.';
          break;
        case 404:
          errorMessage = 'Resource not found!';
          break;
        case 500:
          errorMessage = 'Internal server error! Something went wrong on the server.';
          break;
        default:
          if (error.error?.message) {
            errorMessage = error.error.message;
          }
          break;
      }

      // Log the formatted error to the console as requested in the DoD
      console.error(`[Global Error Interceptor] Status ${error.status}: ${errorMessage}`);

      return throwError(() => new Error(errorMessage));
    })
  );
};