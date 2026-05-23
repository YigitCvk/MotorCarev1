import { Injectable } from '@angular/core';
import { CurrentUser } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  private readonly accessTokenKey = 'motorcare.accessToken';
  private readonly refreshTokenKey = 'motorcare.refreshToken';
  private readonly currentUserKey = 'motorcare.currentUser';
  private readonly clientRateLimitKey = 'motorcare.clientRateLimitKey';

  get accessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  get refreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  get clientRateLimitId(): string {
    const existing = localStorage.getItem(this.clientRateLimitKey);
    if (existing) return existing;

    const value = crypto.randomUUID ? crypto.randomUUID() : `${Date.now()}-${Math.random().toString(36).slice(2)}`;
    localStorage.setItem(this.clientRateLimitKey, value);
    return value;
  }

  setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(this.accessTokenKey, accessToken);
    localStorage.setItem(this.refreshTokenKey, refreshToken);
  }

  setCurrentUser(user: CurrentUser | null): void {
    if (!user) {
      localStorage.removeItem(this.currentUserKey);
      return;
    }

    localStorage.setItem(this.currentUserKey, JSON.stringify(user));
  }

  getCurrentUser(): CurrentUser | null {
    const raw = localStorage.getItem(this.currentUserKey);
    if (!raw) return null;

    try {
      return JSON.parse(raw) as CurrentUser;
    } catch {
      localStorage.removeItem(this.currentUserKey);
      return null;
    }
  }

  clear(): void {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.currentUserKey);
  }
}
