// Import the core Component decorator from the Angular framework
import { Component } from '@angular/core';

@Component({
  // Define the custom HTML selector tag used to render this component
  selector: 'app-sidebar',
  // Enable standalone mode to allow this component to manage its own scope independently
  standalone: true,
  // Declare an empty imports array since no external dependencies are needed here
  imports: [],
  // Specify the external relative file path containing the template design
  templateUrl: './sidebar.html',
  // Specify the external relative file path containing the component styles
  styleUrl: './sidebar.scss'
})
// Export the SidebarComponent class so it can be imported inside app.ts
export class SidebarComponent {
  // Declare a public data matrix containing core navigation labels and design asset icon references
  public navItems = [
    { label: 'Dashboard', icon: 'dashboard' },
    { label: 'Current Account', icon: 'account_balance' },
    { label: 'Identity & Security', icon: 'security' }
  ];
}