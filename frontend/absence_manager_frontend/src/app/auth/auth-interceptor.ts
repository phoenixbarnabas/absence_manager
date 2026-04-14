import { HttpInterceptorFn } from '@angular/common/http';
import { switchMap } from 'rxjs/internal/operators/switchMap';
import { AuthService } from './auth-service';
import { inject } from '@angular/core';
import { from } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  const isApiRequest = req.url.startsWith('https://localhost:7190/api/');

  if (!isApiRequest) {
    return next(req)
  }

  return from(authService.acquireApiToken()).pipe(
    switchMap((token) => {
      if (!token) {
        return next(req)
      }

      const authReq = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });

      return next(authReq)
    })
  )
}
