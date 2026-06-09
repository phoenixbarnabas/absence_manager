import { Component, OnDestroy, OnInit } from '@angular/core';
import { AccountInfo } from '@azure/msal-browser';
import { combineLatest, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { AuthProcessState, AuthService } from '../../auth/auth-service';
import { UserProfile } from '../../models/app-user-models';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.html',
  styleUrl: './navbar.sass',
})
export class Navbar implements OnInit, OnDestroy {
  userProfile: UserProfile | null = null;
  loading = true;
  isLoggedIn = false;
  isCollapsed = false;
  authProcessState: AuthProcessState = 'initializing';

  private readonly sidebarStateStorageKey = 'absence-manager-sidebar-collapsed';
  private readonly destroy$ = new Subject<void>();

  constructor(public authService: AuthService) { }

  ngOnInit(): void {
    this.restoreSidebarState();

    combineLatest([
      this.authService.account$.pipe(
        distinctUntilChanged((previous, current) =>
          previous?.homeAccountId === current?.homeAccountId
        )
      ),
      this.authService.authProcessState$
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([account, authProcessState]) => {
        this.authProcessState = authProcessState;
        this.loading = this.isAuthTransition;

        if (this.isAuthTransition) {
          if (this.isLoggingOut) {
            this.isLoggedIn = false;
            this.userProfile = null;
          }

          return;
        }

        this.isLoggedIn = !!account;

        if (!account) {
          this.userProfile = null;
          return;
        }

        this.userProfile = this.createProfileFromAccount(account);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
    this.saveSidebarState();
  }

  getInitials(displayName: string): string {
    return displayName
      .split(' ')
      .filter(Boolean)
      .map(name => name[0])
      .join('')
      .slice(0, 2)
      .toUpperCase();
  }

  async logout(): Promise<void> {
    await this.authService.logout();
  }

  private createProfileFromAccount(account: AccountInfo): UserProfile {
    return {
      displayName: account.name || account.username || 'Ismeretlen felhasználó',
      email: account.username || '',
      department: '',
      jobTitle: ''
    };
  }

  private restoreSidebarState(): void {
    try {
      this.isCollapsed = localStorage.getItem(this.sidebarStateStorageKey) === 'true';
    } catch {
      this.isCollapsed = false;
    }
  }

  private saveSidebarState(): void {
    try {
      localStorage.setItem(this.sidebarStateStorageKey, String(this.isCollapsed));
    } catch {
      // Ha a localStorage nem elérhető, attól még a navbar működjön.
    }
  }

  get isInitializing(): boolean {
    return this.authProcessState === 'initializing';
  }

  get isLoggingIn(): boolean {
    return this.authProcessState === 'loggingIn';
  }

  get isLoggingOut(): boolean {
    return this.authProcessState === 'loggingOut';
  }

  get isAuthTransition(): boolean {
    return this.isInitializing || this.isLoggingIn || this.isLoggingOut;
  }

  get authTransitionLabel(): string {
    if (this.isLoggingIn) {
      return 'Bejelentkezés...';
    }

    if (this.isLoggingOut) {
      return 'Kijelentkezés...';
    }

    return 'Betöltés...';
  }
}