import { Routes } from '@angular/router';
// Import the auth guard used to block unauthenticated access to protected views
import { authGuard } from './guards/auth.guard';

// Define the comprehensive application tracking route navigation map configuration schema
export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    // Publicly accessible login view, not protected by the auth guard
    path: 'login',
    loadComponent: () => import('./pages/login/login').then(m => m.LoginComponent)
  },
  {
    // Publicly accessible register view, not protected by the auth guard
    path: 'register',
    loadComponent: () => import('./pages/register/register').then(m => m.RegisterComponent)
  },
  {
    // Point directly to the short name convention file for the dashboard component view module
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.DashboardComponent)
  },
  {
    // Register the statement view resource path maps directing layout entry straight to the new page component
    path: 'statement',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/statement/statement').then(m => m.StatementComponent)
  },
  {
    // PDF Upload Centre: centralised screen for all manual PDF imports (statements, payroll, PPR)
    path: 'pdf-upload',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/pdf-upload/pdf-upload').then(m => m.PdfUploadComponent)
  },
  {
    // Lazy-loaded placeholder route for the Current Account module, reached from the Dashboard Hub and Sidebar
    path: 'current-account',
    canActivate: [authGuard],
    data: { moduleName: 'Current Account' },
    loadComponent: () => import('./pages/module-placeholder/module-placeholder').then(m => m.ModulePlaceholderComponent)
  },
  {
    // Lazy-loaded placeholder route for the Investments module, reached from the Dashboard Hub and Sidebar
    path: 'investments',
    canActivate: [authGuard],
    data: { moduleName: 'Investments' },
    loadComponent: () => import('./pages/module-placeholder/module-placeholder').then(m => m.ModulePlaceholderComponent)
  },
  {
    // Lazy-loaded placeholder route for the Pension Plans module, reached from the Dashboard Hub and Sidebar
    path: 'pension-plans',
    canActivate: [authGuard],
    data: { moduleName: 'Pension Plans' },
    loadComponent: () => import('./pages/module-placeholder/module-placeholder').then(m => m.ModulePlaceholderComponent)
  },
  {
    // Lazy-loaded placeholder route for the Savings Certificates module, reached from the Dashboard Hub and Sidebar
    path: 'savings-certificates',
    canActivate: [authGuard],
    data: { moduleName: 'Savings Certificates' },
    loadComponent: () => import('./pages/module-placeholder/module-placeholder').then(m => m.ModulePlaceholderComponent)
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];