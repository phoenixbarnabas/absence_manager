import { Component, OnDestroy, OnInit } from '@angular/core';
import { distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { UserProfile } from '../../models/app-user-models';
import { AuthService } from '../../auth/auth-service';
import { UserService } from '../../services/user.service';
import { AccountInfo } from '@azure/msal-browser';

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

  constructor(public authService: AuthService) { }

  ngOnInit(): void {
    this.authService.account$
      .pipe(
        distinctUntilChanged((previous, current) =>
          previous?.homeAccountId === current?.homeAccountId
        ),
        takeUntil(this.destroy$)
      )
      .subscribe(account => {
        this.isLoggedIn = !!account;

        if (!account) {
          this.userProfile = null;
          this.loading = false;
          return;
        }

        this.userProfile = this.createProfileFromAccount(account);
        this.loading = false;
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
      .map(name => name[0])
      .join('')
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
}