import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { from, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from './auth-service';
import { ConfigService } from '../services/config-service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const configService = inject(ConfigService);

  if (!configService.isLoaded || req.url.endsWith('/config.json')) {
    return next(req);
  }

  const isApiRequest = req.url.startsWith(configService.apiUrl + '/');

  if (!isApiRequest) {
    return next(req);
  }

  return from(authService.acquireApiToken()).pipe(
    switchMap(token => {
      if (!token) {
        router.navigate(['/welcome']);
        return throwError(() => new Error('Nincs hozzáférési token.'));
      }

      const authReq = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });

      return next(authReq).pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status === 401 || error.status === 403) {
            router.navigate(['/welcome']);
          }

          return throwError(() => error);
        })
      );
    })
  );
};