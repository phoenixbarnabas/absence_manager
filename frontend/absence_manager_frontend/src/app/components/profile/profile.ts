import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
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
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loading = true;
    this.error = null;

    const account = this.authService.getAccount() ?? this.authService.getActiveAccount();

    if (!account) {
      this.error = 'Nincs bejelentkezett felhasználó';
      this.loading = false;
      return;
    }

    this.userProfile = {
      displayName: account.name || account.username || 'Ismeretlen felhasználó',
      email: account.username || '',
      department: '',
      jobTitle: ''
    };

    this.userService.getMe().subscribe({
      next: (profile) => {
        this.userProfile = profile;
        this.loading = false;
        this.error = null;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        console.error('Profile load error', err);
        this.error = 'Hiba a profil betöltése közben';
        this.loading = false;
        this.cdr.detectChanges();
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