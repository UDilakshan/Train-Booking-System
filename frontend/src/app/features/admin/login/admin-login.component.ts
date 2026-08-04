import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router } from '@angular/router';
import { ApiError } from '../../../core/models/api-response.model';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-login',
  standalone: true,
  imports: [ReactiveFormsModule, MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  template: `
    <div class="container">
      <mat-card class="login-card">
        <mat-card-header>
          <mat-card-title>Admin Login</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline">
              <mat-label>Email</mat-label>
              <input matInput type="email" formControlName="email" id="email" maxlength="255" autocomplete="username" />
              @if (form.controls.email.hasError('email') && !form.controls.email.hasError('required')) {
                <mat-error>Enter a valid email address</mat-error>
              }
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Password</mat-label>
              <input matInput type="password" formControlName="password" id="password" autocomplete="current-password" />
            </mat-form-field>

            @if (error()) {
              <p class="error">{{ error() }}</p>
            }

            <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || submitting()">
              @if (submitting()) {
                Signing in...
              } @else {
                Sign In
              }
            </button>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [
    `
      .container {
        display: flex;
        justify-content: center;
        padding: 4rem 1.5rem;
      }
      .login-card {
        width: 100%;
        max-width: 24rem;
      }
      form {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      .error {
        color: #b3261e;
        font-size: 0.85rem;
      }
      button {
        margin-top: 0.5rem;
      }
    `,
  ],
})
export class AdminLoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
    password: ['', Validators.required],
  });

  protected submit(): void {
    if (this.form.invalid) return;
    this.submitting.set(true);
    this.error.set(null);
    const { email, password } = this.form.getRawValue();
    const trimmedEmail = email.trim();

    this.authService.login(trimmedEmail, password).subscribe({
      next: () => this.router.navigate(['/admin/dashboard']),
      error: (err: ApiError) => {
        this.submitting.set(false);
        this.error.set(err.message || 'Login failed.');
      },
    });
  }
}
