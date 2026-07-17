// Import the core Component decorator from the Angular framework
import { Component, signal } from '@angular/core';
// Import the RouterOutlet directive to enable dynamic sub-page rendering slots
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
// Import the custom layout shell Header component for global brand presentation
import { HeaderComponent } from './components/header/header';
// Import the custom layout shell Sidebar component for application routing controls
import { SidebarComponent } from './components/sidebar/sidebar';
import { filter } from 'rxjs';

@Component({
  // Define the custom unique HTML selector tag utilized to render the core root application
  selector: 'app-root',
  // Enable standalone mode to allow this component to manage its imports directly
  standalone: true,
  // Inject required routing modules and internal layout components into template visibility scope
  imports: [RouterOutlet, HeaderComponent, SidebarComponent],
  // Specify the external relative file path containing the primary structural template
  templateUrl: './app.html',
  // Specify the external relative file path housing the global layout style rules
  styleUrl: './app.scss'
})
// Export the primary AppComponent class targeting bootstrap initiation
export class AppComponent {
  public readonly showShell = signal(true);

  constructor(private router: Router) {
    this.updateShellVisibility(this.router.url);

    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => this.updateShellVisibility(event.urlAfterRedirects));
  }

  private updateShellVisibility(url: string): void {
    const pathname = (url || window.location.pathname).split('?')[0];
    this.showShell.set(!['/login', '/register'].includes(pathname));
  }
}