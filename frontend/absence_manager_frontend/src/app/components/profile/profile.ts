import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { UserProfile } from '../../models/app-user-models';
import { AuthService } from '../../auth/auth-service';
import { takeUntil } from 'rxjs/internal/operators/takeUntil';
import { Subject } from 'rxjs/internal/Subject';

@Component({
  selector: 'app-profile',
  standalone: false,
  templateUrl: './profile.html',
  styleUrl: './profile.sass',
})
export class Profile implements OnInit, OnDestroy {
  userProfile: UserProfile | null = null;
  loading = true;
  error: string | null = null;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    const account = this.authService.getAccount() ?? this.authService.getActiveAccount();

    if (!account) {
      this.error = 'Nincs bejelentkezett felhasználó.';
      this.loading = false;
      return;
    }

    this.userProfile = {
      displayName: account.name || account.username || 'Ismeretlen felhasználó',
      email: account.username || '',
      department: '',
      jobTitle: ''
    };

    this.userService.getMe()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: profile => {
          this.userProfile = profile;
          this.loading = false;
          this.error = null;
          this.cdr.detectChanges();
        },
        error: err => {
          console.error(err);
          this.error = 'Hiba a profil betöltése közben.';
          this.loading = false;
          this.cdr.detectChanges();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getInitials(displayName: string): string {
    return displayName
      .split(' ')
      .filter(Boolean)
      .map(n => n[0])
      .join('')
      .toUpperCase();
  }

  async logout(): Promise<void> {
    await this.authService.logout();
  }
}