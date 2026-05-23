import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from './auth.service';

export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const roles = route.data['roles'] as string[] | undefined;

  if (auth.hasAnyRole(roles)) {
    return true;
  }

  return auth.loadCurrentUser().pipe(
    map(() => (auth.hasAnyRole(roles) ? true : router.createUrlTree(['/forbidden']))),
    catchError(() => of(router.createUrlTree(['/login'])))
  );
};
