import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { errorInterceptor } from './interceptors/error.interceptor';
// Import the auth interceptor responsible for injecting the bearer token into outgoing requests
import { authInterceptor } from './interceptors/auth.interceptor';

// Establish the primary infrastructure service initialization dependency injector parameters configuration
export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    // Inject the centralized type-safe Angular HTTP client processing client toolset globally inside context
    // The interceptors are passed inside the withInterceptors array, auth runs before the error handler
    provideHttpClient(
      withInterceptors([authInterceptor, errorInterceptor])
    )
  ]
};