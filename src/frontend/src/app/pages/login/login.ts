import { Component, ViewChild, ElementRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

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
    private ngZone: NgZone // <--- ADICIONADO PARA FORÇAR A ATUALIZAÇÃO
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
        this.ngZone.run(() => {
          this.isSubmitting = false;
          this.router.navigate(['/dashboard']);
        });
      },
      error: (err: any) => {
        // FORÇA o Angular a re-renderizar o HTML dentro da Zona
        this.ngZone.run(() => {
          this.errorMessage = err?.error?.detail || 'Invalid email or password. Please try again.';
          this.isSubmitting = false;
        });
      }
    });
  }
}