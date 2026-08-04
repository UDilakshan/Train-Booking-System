import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-site-header',
  standalone: true,
  imports: [RouterLink, MatToolbarModule, MatButtonModule, MatIconModule],
  template: `
    <mat-toolbar color="primary" class="header">
      <a routerLink="/" class="brand">
        <mat-icon>train</mat-icon>
        <span>Sri Lanka Railway</span>
        <span class="subtitle">Colombo Fort – Badulla</span>
      </a>
      <span class="spacer"></span>
      <nav class="nav">
        <a mat-button routerLink="/search">Book a Journey</a>
        <a mat-button routerLink="/my-bookings">My Bookings</a>
        @if (auth.currentUser(); as user) {
          <a mat-button routerLink="/admin/dashboard">Admin ({{ user.name }})</a>
          <button mat-button (click)="logout()">Log Out</button>
        } @else {
          <a mat-button routerLink="/admin/login">Admin</a>
        }
      </nav>
    </mat-toolbar>
  `,
  styles: [
    `
      .header {
        position: sticky;
        top: 0;
        z-index: 10;
      }
      .brand {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        color: inherit;
        text-decoration: none;
        font-weight: 600;
      }
      .subtitle {
        font-weight: 400;
        opacity: 0.85;
        font-size: 0.85rem;
        display: none;
      }
      @media (min-width: 640px) {
        .subtitle {
          display: inline;
        }
      }
      .spacer {
        flex: 1 1 auto;
      }
      .nav {
        display: flex;
        gap: 0.25rem;
      }
    `,
  ],
})
export class SiteHeaderComponent {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
