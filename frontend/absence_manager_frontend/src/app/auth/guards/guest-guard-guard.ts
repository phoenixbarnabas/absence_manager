import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth-service';

export const guestGuardGuard: CanActivateFn = async () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  try {
    await authService.bootstrap();

    if (!authService.isLoggedIn()) {
      return true;
    }

    const token = await authService.acquireApiToken();

    if (token) {
      return router.createUrlTree(['/desk-booking']);
    }

    await authService.clearSessionAfterAuthFailure();
    return true;
  } catch (error) {
    console.error('Guest guard failed.', error);
    await authService.clearSessionAfterAuthFailure();
    return true;
  }
};
