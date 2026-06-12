import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth-service';

export const authGuard: CanActivateFn = async () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  try {
    await authService.bootstrap();
  } catch (error) {
    console.error('Auth bootstrap failed', error);
  }

  if (authService.isLoggedIn()) {
    return true;
  }

  return router.createUrlTree(['/welcome']);
};