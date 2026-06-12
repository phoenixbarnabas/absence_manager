import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { AppUserHierarchyDto, GraphAppUserDto, UserService } from '../../services/user.service';
import { UserProfile } from '../../models/app-user-models';
import { AuthService } from '../../auth/auth-service';
import { takeUntil } from 'rxjs/internal/operators/takeUntil';
import { Subject } from 'rxjs/internal/Subject';
import { AccountInfo } from '@azure/msal-browser';
import { finalize } from 'rxjs/internal/operators/finalize';
import { NotificationService } from '../../services/notification-service';

@Component({
  selector: 'app-profile',
  standalone: false,
  templateUrl: './profile.html',
  styleUrl: './profile.sass',
})
export class Profile implements OnInit, OnDestroy {
  userProfile: UserProfile | null = null;

  manager: GraphAppUserDto | null = null;
  directReports: GraphAppUserDto[] = [];

  loading = true;
  hierarchyLoading = true;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef,
    private notificationService: NotificationService,
  ) { }

  ngOnInit(): void {
    const account = this.authService.getAccount() ?? this.authService.getActiveAccount();

    if (!account) {
      this.notificationService.warning('Nincs bejelentkezett felhasználó.');
      this.loading = false;
      this.hierarchyLoading = false;
      return;
    }

    this.userProfile = this.createProfileFromAccount(account);

    this.loadProfile();
    this.loadHierarchy();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get hasDirectReports(): boolean {
    return this.directReports.length > 0;
  }

  getInitials(displayName: string | null | undefined): string {
    const initials = (displayName || '')
      .split(' ')
      .filter(Boolean)
      .map(name => name[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);

    return initials || '?';
  }

  getUserDisplayName(user: GraphAppUserDto | null | undefined): string {
    return user?.displayName
      || user?.email
      || user?.userPrincipalName
      || 'Ismeretlen felhasználó';
  }

  getUserEmail(user: GraphAppUserDto | null | undefined): string {
    return user?.email || user?.userPrincipalName || '';
  }

  getUserDetails(user: GraphAppUserDto | null | undefined): string {
    const details = [
      user?.jobTitle,
      user?.department
    ].filter(Boolean);

    return details.length ? details.join(' · ') : 'Nincs szervezeti adat';
  }

  trackByUser(_: number, user: GraphAppUserDto): string {
    return user.appUserId || user.entraObjectId || this.getUserDisplayName(user);
  }

  async logout(): Promise<void> {
    await this.authService.logout();
  }

  private loadProfile(): void {
    this.loading = true;

    this.userService.getMe()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: profile => {
          this.userProfile = profile;
        },
        error: err => {
          console.error(err);

          this.notificationService.error(
            this.notificationService.getMessage(
              err,
              'Hiba a profil betöltése közben.'
            )
          );
        }
      });
  }

  private loadHierarchy(): void {
    this.hierarchyLoading = true;

    this.userService.getMyHierarchy()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.hierarchyLoading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: hierarchy => {
          this.applyHierarchy(hierarchy);
        },
        error: err => {
          console.error(err);
          this.manager = null;
          this.directReports = [];

          this.notificationService.error(
            this.notificationService.getMessage(
              err,
              'Hiba a vezetői kapcsolatok betöltése közben.'
            )
          );
        }
      });
  }

  private applyHierarchy(hierarchy: AppUserHierarchyDto): void {
    this.manager = hierarchy.manager ?? null;

    this.directReports = (hierarchy.directReports ?? [])
      .filter(user => user.isActiveLocalUser)
      .sort((a, b) =>
        this.getUserDisplayName(a).localeCompare(
          this.getUserDisplayName(b),
          'hu'
        )
      );
  }

  private createProfileFromAccount(account: AccountInfo): UserProfile {
    return {
      displayName: account.name || account.username || 'Ismeretlen felhasználó',
      email: account.username || '',
      department: '',
      jobTitle: ''
    };
  }
}