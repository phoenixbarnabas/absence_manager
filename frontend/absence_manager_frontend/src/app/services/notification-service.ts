import { Injectable } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';
import { Observable } from 'rxjs/internal/Observable';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface NotificationOptions {
  actionLabel?: string;
  durationMs?: number;
}


@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  constructor(private snackBar: MatSnackBar) { }

  success(message: string, options: NotificationOptions = {}): Observable<void> {
    return this.open('success', message, options);
  }

  error(message: string, options: NotificationOptions = {}): Observable<void> {
    return this.open('error', message, options);
  }

  warning(message: string, options: NotificationOptions = {}): Observable<void> {
    return this.open('warning', message, options);
  }

  info(message: string, options: NotificationOptions = {}): Observable<void> {
    return this.open('info', message, options);
  }

  private open(
    type: NotificationType,
    message: string,
    options: NotificationOptions
  ): Observable<void> {
    const config: MatSnackBarConfig = {
      duration: options.durationMs ?? this.getDefaultDuration(type, options.actionLabel),
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: [
        'app-snackbar',
        `app-snackbar--${type}`
      ]
    };

    const ref = this.snackBar.open(
      message,
      options.actionLabel ?? 'Bezárás',
      config
    );

    return ref.onAction();
  }

  private getDefaultDuration(
    type: NotificationType,
    actionLabel?: string
  ): number {
    if (actionLabel) {
      return 8000;
    }

    if (type === 'error') {
      return 6000;
    }

    if (type === 'warning') {
      return 5000;
    }

    return 4000;
  }

  getMessage(err: any, fallback: string): string {
    if (err?.error?.message) {
      return err.error.message;
    }

    if (err?.status === 0) {
      return 'A backend nem érhető el. Ellenőrizd, hogy fut-e az API és jó-e az apiUrl.';
    }

    if (err?.status === 401) {
      return 'A munkamenet lejárt vagy nincs jogosultságod. Jelentkezz be újra.';
    }

    if (err?.status === 403) {
      return 'Ehhez a művelethez nincs megfelelő jogosultságod.';
    }

    if (err?.name === 'TimeoutError') {
      return 'A backend nem válaszolt időben. Ellenőrizd, hogy fut-e az API.';
    }

    return fallback;
  }
}
