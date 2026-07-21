import { Component, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
// Import TimeoutError to tell a network/backend timeout apart from a genuine auth rejection
import { TimeoutError } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class LoginComponent {
  public loginForm: FormGroup;
  public isSubmitting = false;
  public errorMessage: string | null = null;

  @ViewChild('errorBanner') errorBanner?: ElementRef<HTMLDivElement>;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    // Angular 22 has a known Zone.js change-detection regression (angular/angular#69530)
    // where UI-updating state set inside async callbacks (HTTP errors, etc.) doesn't
    // trigger a repaint even from inside NgZone.run(). ChangeDetectorRef.detectChanges()
    // is the documented workaround until upstream fixes the scheduler.
    private cdr: ChangeDetectorRef
  ) {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  public onSubmit(): void {
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
      error: (err: unknown) => {
        this.isSubmitting = false;
        if (err instanceof TimeoutError) {
          this.errorMessage = 'The server took too long to respond. Please check your connection and try again.';
        } else {
          const httpError = err as { error?: { detail?: string } };
          this.errorMessage = httpError?.error?.detail || 'Invalid email or password. Please try again.';
        }
        // Force a repaint immediately — see constructor comment on the Angular 22 CD regression
        this.cdr.detectChanges();
      }
    });
  }
}
