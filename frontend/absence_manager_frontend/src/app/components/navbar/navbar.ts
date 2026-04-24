import { Component, OnDestroy, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Subject, filter, takeUntil } from 'rxjs';
import { UserProfile } from '../../models/app-user-models';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../auth/auth-service';

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
  isLoginPage = false;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private userService: UserService,
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.updateRouteState();

    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => this.updateRouteState());

    this.authService.account$
      .pipe(takeUntil(this.destroy$))
      .subscribe(account => {
        this.isLoggedIn = !!account;

        if (!account) {
          this.userProfile = null;
          this.loading = false;
          return;
        }

        this.userProfile = {
          displayName: account.name || account.username || 'Ismeretlen felhasználó',
          email: account.username || '',
          department: '',
          jobTitle: ''
        };

        this.loading = false;
        this.loadProfileData();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateRouteState(): void {
    this.isLoginPage = this.router.url.startsWith('/login');
  }

  private loadProfileData(): void {
    this.userService.getMe().subscribe({
      next: profile => {
        this.userProfile = profile;
      },
      error: err => {
        console.error(err);
      }
    });
  }

  getInitials(displayName: string): string {
    return displayName
      .split(' ')
      .filter(Boolean)
      .map(name => name[0])
      .join('')
      .toUpperCase();
  }

  async logout(): Promise<void> {
    await this.authService.logout();
  }
}