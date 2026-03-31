import { ChangeDetectorRef, Component, HostListener, OnInit } from '@angular/core';
import { UserProfile } from '../../models/app-user-models';
import { UserService } from '../../services/user.service';
import { DevAuthService } from '../../services/dev-auth-service';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.html',
  styleUrl: './navbar.sass',
})
export class Navbar implements OnInit {
  userProfile: UserProfile | null = null;

  constructor(
    private userService: UserService,
    private devAuthService: DevAuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadFromCurrentDevUser();
    this.loadProfileData();
  }

  @HostListener('window:dev-user-changed')
  onDevUserChanged(): void {
    this.loadFromCurrentDevUser();
    this.loadProfileData();
    this.cdr.detectChanges();
  }

  private loadFromCurrentDevUser(): void {
    const currentUser = this.devAuthService.getCurrentUser();

    if (!currentUser) {
      this.userProfile = null;
      this.cdr.detectChanges();
      return;
    }

    this.userProfile = {
      displayName: currentUser.displayName,
      email: currentUser.email,
      department: currentUser.department,
      jobTitle: currentUser.jobTitle
    };

    this.cdr.detectChanges();
  }

  private loadProfileData(): void {
    this.userService.getMe().subscribe({
      next: (profile) => {
        this.userProfile = profile;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        this.cdr.detectChanges();
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