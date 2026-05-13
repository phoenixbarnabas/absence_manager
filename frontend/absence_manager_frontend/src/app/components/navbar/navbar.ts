import { Component, OnDestroy, OnInit } from '@angular/core';
import { distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { UserProfile } from '../../models/app-user-models';
import { AuthService } from '../../auth/auth-service';
import { UserService } from '../../services/user.service';

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

  private readonly destroy$ = new Subject<void>();
  private profileLoaded = false;

  constructor(
    private userService: UserService,
    public authService: AuthService
  ) { }

  ngOnInit(): void {
    this.authService.account$
      .pipe(
        distinctUntilChanged((prev, curr) =>
          prev?.homeAccountId === curr?.homeAccountId
        ),
        takeUntil(this.destroy$)
      )
      .subscribe((account) => {
        this.isLoggedIn = !!account;

        if (!account) {
          this.userProfile = null;
          this.profileLoaded = false;
          this.loading = false;
          return;
        }

        if (!this.profileLoaded && !this.userProfile) {
          this.userProfile = {
            displayName: account.name || account.username || 'Ismeretlen felhasználó',
            email: account.username || '',
            department: '',
            jobTitle: ''
          };
        }

        this.loading = false;
        this.loadProfileData();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadProfileData(): void {
    this.userService.getMe().subscribe({
      next: (profile) => {
        this.userProfile = profile;
        this.profileLoaded = true;
      },
      error: (err) => {
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