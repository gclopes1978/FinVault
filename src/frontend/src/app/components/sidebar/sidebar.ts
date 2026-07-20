// Import the core Component decorator from the Angular framework
import { Component } from '@angular/core';
// Import RouterLink and RouterLinkActive to enable declarative navigation and active-state highlighting
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  // Define the custom HTML selector tag used to render this component
  selector: 'app-sidebar',
  // Enable standalone mode to allow this component to manage its own scope independently
  standalone: true,
  // Inject the router directives required for links and active-route styling in the template
  imports: [RouterLink, RouterLinkActive],
  // Specify the external relative file path containing the template design
  templateUrl: './sidebar.html',
  // Specify the external relative file path containing the component styles
  styleUrl: './sidebar.scss'
})
// Export the SidebarComponent class so it can be imported inside app.ts
export class SidebarComponent {
  // Declare a public data matrix containing every core navigation destination for the Dashboard Hub
  public navItems = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    { label: 'Current Account', icon: 'account_balance', route: '/current-account' },
    { label: 'Investments', icon: 'trending_up', route: '/investments' },
    { label: 'Pension Plans', icon: 'savings', route: '/pension-plans' },
    { label: 'Savings Certificates', icon: 'workspace_premium', route: '/savings-certificates' }
  ];
}