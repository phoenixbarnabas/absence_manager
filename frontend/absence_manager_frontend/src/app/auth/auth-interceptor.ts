import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { from, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from './auth-service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isApiRequest = req.url.startsWith('https://localhost:7190/api/');

  if (!isApiRequest) {
    return next(req);
  }

  return from(authService.acquireApiToken()).pipe(
    switchMap(token => {
      if (!token) {
        router.navigate(['/login']);
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
            router.navigate(['/login']);
          }

          return throwError(() => error);
        })
      );
    })
  );
};