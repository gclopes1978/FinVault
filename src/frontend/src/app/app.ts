// Import the core Component decorator from the Angular framework
import { Component } from '@angular/core';
// Import the custom HeaderComponent class from its local directory file path
import { HeaderComponent } from './components/header/header';
// Import the custom SidebarComponent class from its local directory file path
import { SidebarComponent } from './components/sidebar/sidebar';

@Component({
  // Define the custom HTML selector tag used to render this root component
  selector: 'app-root',
  // Enable standalone component mode to manage its own imports without NgModules
  standalone: true,
  // Inject the required Header and Sidebar components into this component context
  imports: [HeaderComponent, SidebarComponent],
  // Specify the external relative file path containing the component's HTML template
  templateUrl: './app.html',
  // Specify the external relative file path containing the component's SCSS styles
  styleUrl: './app.scss'
})
// Export the main AppComponent class so it can be bootstrapped by main.ts
export class AppComponent {
  // Main core application shell orchestrating layout structure
}