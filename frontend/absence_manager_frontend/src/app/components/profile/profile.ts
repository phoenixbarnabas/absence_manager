import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { UserProfile } from '../../models/app-user-models';
import { DevAuthService } from '../../services/dev-auth-service';
import { AuthService } from '../../auth/auth-service';

@Component({
  selector: 'app-profile',
  standalone: false,
  templateUrl: './profile.html',
  styleUrl: './profile.sass',
})
export class Profile implements OnInit {
   userProfile: UserProfile | null = null;
  loading = true;
  error: string | null = null;

  constructor(
    private userService: UserService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const account = this.authService.getAccount() ?? this.authService.getActiveAccount();

    if (account) {
      this.userProfile = {
        displayName: account.name || account.username || 'Ismeretlen felhasználó',
        email: account.username || '',
        department: '',
        jobTitle: ''
      };
      this.loading = false;
    }

    if (!this.authService.isLoggedIn()) {
      this.error = 'Nincs bejelentkezett felhasználó';
      this.loading = false;
      return;
    }

    this.loadProfileData();
  }

  private loadProfileData(): void {
    this.userService.getMe().subscribe({
      next: (profile) => {
        this.userProfile = profile;
        this.loading = false;
        this.error = null;
      },
      error: (err) => {
        console.error(err);

        if (!this.userProfile) {
          this.error = 'Hiba a profil betöltése közben';
        }

        this.loading = false;
      }
    });
  }

  getInitials(displayName: string): string {
    return displayName
      .split(' ')
      .filter(Boolean)
      .map(n => n[0])
      .join('')
      .toUpperCase();
  }
}