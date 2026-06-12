import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth-service';

export const authGuard: CanActivateFn = async () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  try {
    await authService.bootstrap();

    const token = await authService.acquireApiToken();

    if (token) {
      return true;
    }

    await authService.clearSessionAfterAuthFailure();
  } catch (error) {
    console.error('Auth guard failed.', error);
    await authService.clearSessionAfterAuthFailure();
  }

  return router.createUrlTree(['/welcome']);
};