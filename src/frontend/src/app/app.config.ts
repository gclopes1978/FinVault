import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { errorInterceptor } from './interceptors/error.interceptor';

// Establish the primary infrastructure service initialization dependency injector parameters configuration
export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    // Inject the centralized type-safe Angular HTTP client processing client toolset globally inside context
    // The interceptor is passed inside the withInterceptors array
    provideHttpClient(
      withInterceptors([errorInterceptor])
    )
  ]
};