import { Component, OnDestroy, OnInit } from '@angular/core';
import { UserProfile } from '../../models/app-user-models';
import { UserService } from '../../services/user.service';
import { DevAuthService } from '../../services/dev-auth-service';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.html',
  styleUrl: './navbar.sass',
})
export class Navbar implements OnInit, OnDestroy {
  userProfile: UserProfile | null = null;

  private readonly onDevUserChanged = () => {
    this.loadFromCurrentDevUser();
    this.loadProfileData();
  };

  constructor(
    private userService: UserService,
    private devAuthService: DevAuthService
  ) {}

  ngOnInit(): void {
    this.loadFromCurrentDevUser();
    this.loadProfileData();

    window.addEventListener('dev-user-changed', this.onDevUserChanged);
  }

  ngOnDestroy(): void {
    window.removeEventListener('dev-user-changed', this.onDevUserChanged);
  }

  private loadFromCurrentDevUser(): void {
    const currentUser = this.devAuthService.getCurrentUser();

    if (!currentUser) {
      this.userProfile = null;
      return;
    }

    this.userProfile = {
      displayName: currentUser.displayName,
      email: currentUser.email,
      department: currentUser.department,
      jobTitle: currentUser.jobTitle
    };
  }

  private loadProfileData(): void {
    this.userService.getMe().subscribe({
      next: (profile) => {
        this.userProfile = profile;
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
}