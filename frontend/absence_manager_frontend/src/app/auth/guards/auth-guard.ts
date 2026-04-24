import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SessionService } from '../../services/session-service';

export const authGuard: CanActivateFn = async (_route, state) => {
  const sessionService = inject(SessionService);
  const router = inject(Router);

  try {
    await sessionService.init();
  } catch (error) {
    console.error('Auth guard init failed', error);
  }

  if (sessionService.isLoggedIn) {
    return true;
  }

  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url }
  });
};