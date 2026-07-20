// Import the core Component decorator and lifecycle hook contract from the Angular framework
import { Component, OnInit } from '@angular/core';
// Import the CommonModule to enable standard structural directives inside the template
import { CommonModule } from '@angular/common';
// Import ActivatedRoute to resolve the static route data configured for each lazy-loaded module
import { ActivatedRoute } from '@angular/router';

@Component({
  // Define the custom HTML selector tag used to render this reusable placeholder page
  selector: 'app-module-placeholder',
  // Enable standalone mode to allow this component to manage its own scope independently
  standalone: true,
  // Inject CommonModule into the imports array to utilize built-in template features
  imports: [CommonModule],
  // Specify the external relative file path containing the template layout
  templateUrl: './module-placeholder.html',
  // Specify the external relative file path containing the page styles
  styleUrl: './module-placeholder.scss'
})
// Export the ModulePlaceholderComponent so it can be reused across every "Under Construction" route
export class ModulePlaceholderComponent implements OnInit {
  // Store the human-readable module name resolved from the active route static data
  public moduleName: string = 'Module';

  constructor(private route: ActivatedRoute) {}

  // Resolve the module name from the route data supplied at registration time in app.routes.ts
  public ngOnInit(): void {
    this.moduleName = this.route.snapshot.data['moduleName'] ?? 'Module';
  }
}
