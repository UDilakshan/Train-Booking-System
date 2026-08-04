import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { AuthUser, LoginResult } from '../models/auth.model';
import { ApiService } from './api.service';
import { TokenStorageService } from './token-storage.service';

const USER_KEY = 'railway_admin_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly tokenStorage = inject(TokenStorageService);

  readonly currentUser = signal<AuthUser | null>(this.readStoredUser());

  get isAuthenticated(): boolean {
    return !!this.tokenStorage.getToken();
  }

  login(email: string, password: string): Observable<LoginResult> {
    return this.api.post<LoginResult>('auth/login', { email, password }).pipe(
      tap((result) => {
        this.tokenStorage.setToken(result.accessToken);
        const user: AuthUser = { userId: result.userId, email: result.email, name: result.name, role: result.role };
        localStorage.setItem(USER_KEY, JSON.stringify(user));
        this.currentUser.set(user);
      }),
    );
  }

  logout(): void {
    this.tokenStorage.clearToken();
    localStorage.removeItem(USER_KEY);
    this.currentUser.set(null);
  }

  private readStoredUser(): AuthUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as AuthUser;
    } catch {
      return null;
    }
  }
}
