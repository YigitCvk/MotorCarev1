import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { TokenStorageService } from './token-storage.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const storage = inject(TokenStorageService);
  const auth = inject(AuthService);
  const accessToken = storage.accessToken;
  const clientKey = storage.clientRateLimitId;
  const isAuthRefresh = request.url.includes('/api/auth/refresh-token');
  const isAuthPublic =
    request.url.includes('/api/auth/login') ||
    request.url.includes('/api/auth/register') ||
    request.url.includes('/api/auth/forgot-password') ||
    request.url.includes('/api/auth/reset-password') ||
    request.url.includes('/api/auth/verify-email') ||
    request.url.includes('/api/auth/two-factor') ||
    request.url.includes('/api/auth/accept-invite');

  let nextRequest = request.clone({
    setHeaders: {
      'X-MotorCare-Client-Key': clientKey
    }
  });

  if (accessToken && !isAuthPublic) {
    nextRequest = nextRequest.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`,
        'X-MotorCare-Client-Key': clientKey
      }
    });
  }

  return next(nextRequest).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        accessToken &&
        !isAuthRefresh &&
        !isAuthPublic
      ) {
        return auth.refresh().pipe(
          switchMap((response) =>
            next(
              request.clone({
                setHeaders: {
                  Authorization: `Bearer ${response.accessToken}`,
                  'X-MotorCare-Client-Key': clientKey
                }
              })
            )
          ),
          catchError((refreshError: unknown) => {
            auth.logoutLocal();
            return throwError(() => refreshError);
          })
        );
      }

      return throwError(() => error);
    })
  );
};
