import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, finalize, map, of, shareReplay, tap, throwError } from 'rxjs';
import { ApiService } from '../api/api.service';
import { CurrentUser, LoginRequest, LoginResponse } from '../models/api.models';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private refreshInFlight$?: Observable<LoginResponse>;
  private readonly currentUserSubject = new BehaviorSubject<CurrentUser | null>(this.storage.getCurrentUser());

  readonly currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private readonly api: ApiService,
    private readonly storage: TokenStorageService
  ) {}

  get isAuthenticated(): boolean {
    return !!this.storage.accessToken;
  }

  get currentUser(): CurrentUser | null {
    return this.currentUserSubject.value;
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.api.post<LoginResponse>('/api/auth/login', request).pipe(
      tap((response) => {
        if (!response.requiresTwoFactor) {
          this.applyLogin(response);
        }
      })
    );
  }

  register(body: unknown): Observable<unknown> {
    return this.api.post('/api/auth/register', body);
  }

  verifyEmail(body: unknown): Observable<unknown> {
    return this.api.post('/api/auth/verify-email-code', body);
  }

  forgotPassword(body: unknown): Observable<unknown> {
    return this.api.post('/api/auth/forgot-password', body);
  }

  resetPassword(body: unknown): Observable<unknown> {
    return this.api.post('/api/auth/reset-password', body);
  }

  acceptInvite(body: unknown): Observable<unknown> {
    return this.api.post('/api/auth/accept-invite', body);
  }

  verifyTwoFactor(body: unknown): Observable<LoginResponse> {
    return this.api.post<LoginResponse>('/api/auth/two-factor/verify', body).pipe(tap((response) => this.applyLogin(response)));
  }

  refresh(): Observable<LoginResponse> {
    const refreshToken = this.storage.refreshToken;
    if (!refreshToken) {
      this.logoutLocal();
      return throwError(() => new Error('Refresh token is missing.'));
    }

    if (!this.refreshInFlight$) {
      this.refreshInFlight$ = this.api.post<LoginResponse>('/api/auth/refresh-token', { refreshToken }).pipe(
        tap((response) => this.applyLogin(response)),
        finalize(() => {
          this.refreshInFlight$ = undefined;
        }),
        shareReplay({ bufferSize: 1, refCount: false })
      );
    }

    return this.refreshInFlight$;
  }

  loadCurrentUser(): Observable<CurrentUser | null> {
    if (!this.storage.accessToken) {
      return of(null);
    }

    return this.api.get<CurrentUser>('/api/auth/me').pipe(
      tap((user) => {
        this.storage.setCurrentUser(user);
        this.currentUserSubject.next(user);
      }),
      map((user) => user),
      catchError(() => of(this.currentUser))
    );
  }

  logout(): void {
    const refreshToken = this.storage.refreshToken;
    if (refreshToken) {
      this.api.post('/api/auth/logout', { refreshToken }).subscribe({ error: () => undefined });
    }

    this.logoutLocal();
  }

  logoutLocal(): void {
    this.storage.clear();
    this.currentUserSubject.next(null);
  }

  roleLanding(role = this.currentUser?.role): string {
    switch (role) {
      case 'Technician':
        return '/service-orders';
      case 'Inspector':
        return '/inspections';
      case 'ReadOnly':
        return '/customers';
      default:
        return '/dashboard';
    }
  }

  hasAnyRole(allowedRoles: string[] | undefined): boolean {
    if (!allowedRoles || allowedRoles.length === 0) return true;
    const role = this.currentUser?.role;
    return !!role && allowedRoles.includes(role);
  }

  private applyLogin(response: LoginResponse): void {
    if (!response.accessToken || !response.refreshToken) return;

    this.storage.setTokens(response.accessToken, response.refreshToken);
    const user: CurrentUser = {
      userId: response.userId,
      tenantId: response.tenantId,
      tenantIdentifier: response.tenantIdentifier,
      email: response.email,
      role: response.role
    };
    this.storage.setCurrentUser(user);
    this.currentUserSubject.next(user);
  }
}
