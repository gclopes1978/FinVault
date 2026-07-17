// Import the core Component decorator from the Angular framework
import { Component, signal } from '@angular/core';
// Import the CommonModule to enable standard structural directives inside the template
import { CommonModule } from '@angular/common';
// Import the Reactive Forms building blocks used to validate the registration form
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
// Import the Router and RouterLink to navigate back to the login page after a successful registration
import { Router, RouterLink } from '@angular/router';
// Import the AuthService that performs the actual registration HTTP call
import { AuthService } from '../../services/auth.service';

@Component({
  // Define the custom HTML selector tag used to render this page component
  selector: 'app-register',
  // Enable standalone mode to allow this component to manage its own scope independently
  standalone: true,
  // Inject the modules required to build and validate the reactive registration form
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  // Specify the external relative file path containing the template layout
  templateUrl: './register.html',
  // Specify the external relative file path containing the page styles
  styleUrl: './register.scss'
})
// Export the RegisterComponent class so it can be registered in the application routing
export class RegisterComponent {
  // Declare the reactive form group tracking the email, password and confirmPassword controls
  public registerForm: FormGroup;

  // Track whether the registration request is currently in flight to disable the submit button
  public isSubmitting = false;

  // Hold the error message returned by a failed registration attempt
  public errorMessage: string | null = null;

  public readonly passwordStrength = signal<'weak' | 'medium' | 'strong' | 'very-strong' | null>(null);
  public readonly passwordStrengthLabel = signal<string>('');
  public readonly passwordStrengthClass = signal<string>('');

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.formBuilder.group(
      {
        fullName: ['', [Validators.required, Validators.minLength(2)]],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(8)]],
        confirmPassword: ['', [Validators.required]]
      },
      {
        validators: this.passwordsMatchValidator
      }
    );

    this.registerForm.get('password')?.valueChanges.subscribe((value: string | null) => {
      this.updatePasswordStrength(value ?? '');
    });
  }

  public onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = null;

    const { fullName, email, password, confirmPassword } = this.registerForm.value;

    this.authService.register({ fullName, email, password, confirmPassword }).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.router.navigate(['/login']);
      },
      error: () => {
        this.isSubmitting = false;
        this.errorMessage = 'We could not create your account right now. Please try again.';
      }
    });
  }

  private passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;

    return password && confirmPassword && password !== confirmPassword
      ? { passwordsMismatch: true }
      : null;
  }

  private updatePasswordStrength(password: string): void {
    if (!password) {
      this.passwordStrength.set(null);
      this.passwordStrengthLabel.set('');
      this.passwordStrengthClass.set('');
      return;
    }

    const hasLower = /[a-z]/.test(password);
    const hasUpper = /[A-Z]/.test(password);
    const hasNumber = /\d/.test(password);
    const hasSpecial = /[^A-Za-z0-9]/.test(password);
    const score = Number(hasLower) + Number(hasUpper) + Number(hasNumber) + Number(hasSpecial) + Number(password.length >= 8);

    if (score <= 2) {
      this.passwordStrength.set('weak');
      this.passwordStrengthLabel.set('Weak password');
      this.passwordStrengthClass.set('meter-weak');
    } else if (score <= 4) {
      this.passwordStrength.set('medium');
      this.passwordStrengthLabel.set('Fair password');
      this.passwordStrengthClass.set('meter-medium');
    } else if (score <= 5) {
      this.passwordStrength.set('strong');
      this.passwordStrengthLabel.set('Strong password');
      this.passwordStrengthClass.set('meter-strong');
    } else {
      this.passwordStrength.set('very-strong');
      this.passwordStrengthLabel.set('Excellent password');
      this.passwordStrengthClass.set('meter-very-strong');
    }
  }
}
