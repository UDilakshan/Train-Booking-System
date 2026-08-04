import { Injectable } from '@angular/core';

const TOKEN_KEY = 'railway_admin_token';

/**
 * localStorage-based token storage. The original Next.js build used an httpOnly-cookie proxy
 * (Next API routes gave it a server layer to do that through); a pure Angular SPA has no such
 * layer without standing up a separate BFF, so this is the pragmatic tradeoff — documented in
 * README "Tradeoffs" alongside the XSS-exposure caveat that comes with it.
 */
@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  setToken(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
  }

  clearToken(): void {
    localStorage.removeItem(TOKEN_KEY);
  }
}
