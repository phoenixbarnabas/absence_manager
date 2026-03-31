import { Component, OnInit } from '@angular/core';
import { UserProfile, UserService } from '../../services/user.service';

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

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.loadProfileData();
  }

  private loadProfileData(): void {
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