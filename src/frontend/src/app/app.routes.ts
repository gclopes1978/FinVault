// Import the Routes configuration type from the core Angular router module
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    // Define the default empty path route to automatically catch root applications URLs
    path: '',
    // Use dynamic lazy loading to import the DashboardComponent asynchronously upon request
    loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.DashboardComponent),
    // Define matching configuration criteria to require an absolute exact path matching check
    pathMatch: 'full'
  }
];