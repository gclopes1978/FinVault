import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { LoginComponent } from './login';
import { AuthService } from '../../services/auth.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceStub: Partial<AuthService>;
  let routerStub: Partial<Router>;

  beforeEach(async () => {
    authServiceStub = { login: jest.fn() };
    routerStub = { navigate: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [LoginComponent, HttpClientTestingModule],
      providers: [
        { provide: AuthService, useValue: authServiceStub },
        { provide: Router, useValue: routerStub }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should mark the form as touched and skip submission when invalid', () => {
    component.onSubmit();

    expect(authServiceStub.login).not.toHaveBeenCalled();
    expect(component.loginForm.touched).toBe(true);
  });

  it('should navigate to the dashboard on successful login', () => {
    (authServiceStub.login as jest.Mock).mockReturnValue(
      of({ accessToken: 'fake-jwt-token', expiration: '2026-01-01T00:00:00Z' })
    );
    component.loginForm.setValue({ email: 'user@example.com', password: 'secret123' });

    component.onSubmit();

    expect(authServiceStub.login).toHaveBeenCalledWith({
      email: 'user@example.com',
      password: 'secret123'
    });
    expect(routerStub.navigate).toHaveBeenCalledWith(['/dashboard']);
    expect(component.isSubmitting).toBe(false);
  });

  it('should surface an error message when the credentials are rejected', () => {
    (authServiceStub.login as jest.Mock).mockReturnValue(throwError(() => new Error('Unauthorized')));
    component.loginForm.setValue({ email: 'user@example.com', password: 'wrongpass' });

    component.onSubmit();

    expect(component.errorMessage).toBe('Invalid email or password. Please try again.');
    expect(component.isSubmitting).toBe(false);
    expect(routerStub.navigate).not.toHaveBeenCalled();
  });
});
