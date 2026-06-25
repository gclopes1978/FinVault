// Import the core Component decorator from the Angular framework
import { Component } from '@angular/core';
// Import the CommonModule to enable standard Angular directives like CurrencyPipe
import { CommonModule } from '@angular/common';

@Component({
  // Define the custom HTML selector tag used to render this page component
  selector: 'app-dashboard',
  // Enable standalone mode to allow this component to manage its own scope independently
  standalone: true,
  // Inject CommonModule into the imports array to utilize built-in template features
  imports: [CommonModule],
  // Specify the external relative file path containing the template layout
  templateUrl: './dashboard.html',
  // Specify the external relative file path containing the page styles
  styleUrl: './dashboard.scss'
})
// Export the DashboardComponent class so it can be registered in the application routing
export class DashboardComponent {
  // Define a public mock numeric value representing the user's total current balance
  public currentBalance: number = 24500.75;
  // Define a public mock numeric value representing the total monthly income received
  public monthlyIncome: number = 4800.00;
  // Define a public mock numeric value representing the total monthly expenses incurred
  public monthlyExpenses: number = 1245.30;
}