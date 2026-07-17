// Import the core Component decorator from the Angular framework
import { Component } from '@angular/core';
// Import the CommonModule to enable standard structural directives inside the template
import { CommonModule } from '@angular/common';
// Import the Reactive Forms building blocks used to validate the credentials form
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
// Import the Router to navigate to the dashboard after a successful authentication
import { Router, RouterLink } from '@angular/router';
// Import the AuthService that performs the actual login HTTP call
import { AuthService } from '../../services/auth.service';

@Component({
  // Define the custom HTML selector tag used to render this page component
  selector: 'app-login',
  // Enable standalone mode to allow this component to manage its own scope independently
  standalone: true,
  // Inject the modules required to build and validate the reactive login form
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  // Specify the external relative file path containing the template layout
  templateUrl: './login.html',
  // Specify the external relative file path containing the page styles
  styleUrl: './login.scss'
})
// Export the LoginComponent class so it can be registered in the application routing
export class LoginComponent {
  // Declare the reactive form group tracking the email and password controls
  public loginForm: FormGroup;

  // Track whether a login request is currently in flight to disable the submit button
  public isSubmitting = false;

  // Hold the error message returned by a failed authentication attempt
  public errorMessage: string | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    // Build the form here, once formBuilder has been injected by Angular
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  // Submit the credentials to the AuthService and redirect to the dashboard on success
  public onSubmit(): void {
    // Do nothing when the form is invalid, letting field-level errors guide the user
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = null;

    const { email, password } = this.loginForm.value;

    this.authService.login({ email, password }).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.isSubmitting = false;
        this.errorMessage = 'Invalid email or password. Please try again.';
      }
    });
  }
}
