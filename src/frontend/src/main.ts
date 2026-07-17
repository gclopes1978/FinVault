import 'zone.js';

// Import the bootstrapApplication function to initialize the Angular standalone app
import { bootstrapApplication } from '@angular/platform-browser';
// Import the appConfig configuration object for global providers and routing
import { appConfig } from './app/app.config';
// Import the correct root AppComponent from the application file path
import { AppComponent } from './app/app';

// Bootstrap the standalone application using the root AppComponent and configuration
bootstrapApplication(AppComponent, appConfig)
  // Catch and log any initialization errors to the browser console environment
  .catch((err) => console.error(err));