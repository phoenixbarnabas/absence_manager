import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth-service';

export const guestGuardGuard: CanActivateFn = async () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  try {
    await authService.bootstrap();
  } catch (error) {
    console.error('Guest guard bootstrap failed', error);
    await authService.clearSessionAfterAuthFailure();
    return true;
  }

  if (authService.isLoggedIn()) {
    return router.createUrlTree(['/desk-booking']);
  }

  return true;
};
