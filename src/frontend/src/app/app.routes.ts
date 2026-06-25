import { Routes } from '@angular/router';

// Define the comprehensive application tracking route navigation map configuration schema
export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    // Point directly to the short name convention file for the dashboard component view module
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.DashboardComponent)
  },
  {
    // Register the statement view resource path maps directing layout entry straight to the new page component
    path: 'statement',
    loadComponent: () => import('./pages/statement/statement').then(m => m.StatementComponent)
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];