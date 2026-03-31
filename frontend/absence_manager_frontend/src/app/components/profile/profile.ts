import { Component, OnInit, OnDestroy } from '@angular/core';
import { UserService } from '../../services/user.service';
import { UserProfile } from '../../models/app-user-models';

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

  private readonly onDevUserChanged = () => {
    this.loadProfileData();
  };

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.loadProfileData();
    window.addEventListener('dev-user-changed', this.onDevUserChanged);
  }

  ngOnDestroy(): void {
    window.removeEventListener('dev-user-changed', this.onDevUserChanged);
  }

  private loadProfileData(): void {
    console.log('PROFILE LOAD START');
    console.log('LOCAL USER', localStorage.getItem('dev-auth-user'));
    console.log('LOCAL TOKEN', localStorage.getItem('dev-auth-token'));
    this.loading = true;
    this.error = null;

    this.userService.getMe().subscribe({
      next: (profile) => {
        this.userProfile = profile;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Hiba a profil betöltése közben';
        this.loading = false;
        console.error(err);
      }
    });
  }

  getInitials(displayName: string): string {
    return displayName
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase();
  }

  openSettings(): void {
    console.log('Open settings');
  }
}