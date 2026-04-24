import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { UserProfile } from '../../models/app-user-models';
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
    const account = this.authService.getAccount();

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

    this.userService.getMe().subscribe({
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