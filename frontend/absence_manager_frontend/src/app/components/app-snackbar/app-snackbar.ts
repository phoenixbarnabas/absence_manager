import { Component, Inject } from '@angular/core';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';
import { Subject } from 'rxjs/internal/Subject';
import { NotificationType } from '../../services/notification-service';

export interface AppSnackbarData {
  type: NotificationType;
  message: string;
  actionLabel?: string;
  actionSubject: Subject<void>;
}


@Component({
  selector: 'app-app-snackbar',
  standalone: false,
  templateUrl: './app-snackbar.html',
  styleUrl: './app-snackbar.sass',
})
export class AppSnackbar {
  constructor(
    @Inject(MAT_SNACK_BAR_DATA) public data: AppSnackbarData,
    private snackBarRef: MatSnackBarRef<AppSnackbar>
  ) { }

  runAction(): void {
    this.data.actionSubject.next();
    this.data.actionSubject.complete();
    this.snackBarRef.dismiss();
  }

  close(): void {
    this.data.actionSubject.complete();
    this.snackBarRef.dismiss();
  }

  getIcon(): string {
    switch (this.data.type) {
      case 'success':
        return 'bi-check-circle';
      case 'error':
        return 'bi-exclamation-triangle';
      case 'warning':
        return 'bi-exclamation-circle';
      case 'info':
      default:
        return 'bi-info-circle';
    }
  }
}
