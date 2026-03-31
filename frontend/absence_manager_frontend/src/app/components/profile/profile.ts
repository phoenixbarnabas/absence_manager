import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { UserProfile } from '../../models/app-user-models';
import { DevAuthService } from '../../services/dev-auth-service';

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

  constructor(private userService: UserService, private devAuthService: DevAuthService) {}

  ngOnInit(): void {
    this.loadFromCurrentDevUser();
    this.loadProfileData();
  }

  private loadFromCurrentDevUser(): void {
    const currentUser = this.devAuthService.getCurrentUser();

    if (!currentUser) {
      return;
    }

    this.userProfile = {
      displayName: currentUser.displayName,
      email: currentUser.email,
      department: currentUser.department,
      jobTitle: currentUser.jobTitle
    };

    this.loading = false;
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
      .map(n => n[0])
      .join('')
      .toUpperCase();
  }

  openSettings(): void {
    console.log('Open settings');
  }
}