// Import the core Component decorator from the Angular framework
import { Component } from '@angular/core';

@Component({
  // Define the custom HTML selector tag used to render this component
  selector: 'app-header',
  // Enable standalone mode to allow this component to manage its own scope independently
  standalone: true,
  // Declare an empty imports array since no external dependencies are needed here
  imports: [],
  // Specify the external relative file path containing the template design
  templateUrl: './header.html',
  // Specify the external relative file path containing the component styles
  styleUrl: './header.scss'
})
// Export the HeaderComponent class so it can be imported inside app.ts
export class HeaderComponent {
  // Define a public property storing a default placeholder username for display purposes
  public username: string = 'G. Lopes';
}