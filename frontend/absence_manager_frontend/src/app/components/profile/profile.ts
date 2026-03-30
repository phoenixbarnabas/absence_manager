import { Component, OnInit } from '@angular/core';
import { LeaveBalance, UserProfile, UserService } from '../../services/user.service';

@Component({
  selector: 'app-profile',
  standalone: false,
  templateUrl: './profile.html',
  styleUrl: './profile.sass',
})
export class Profile implements OnInit {
  userProfile: UserProfile | null = null;
  leaveBalance: LeaveBalance | null = null;
  loading = true;
  error: string | null = null;

  constructor(private userService: UserService) { }

  ngOnInit(): void {
    this.loadProfileData();
  }

  private loadProfileData(): void {
    const userId = 'current-user'; // TODO: Get from auth service

    this.userService.getUserProfile(userId).subscribe({
      next: (profile) => {
        this.userProfile = profile;
        this.loadLeaveBalance(userId);
      },
      error: (err) => {
        this.error = 'Hiba a profil betöltése közben';
        this.loading = false;
        console.error(err);
      }
    });
  }

  private loadLeaveBalance(userId: string): void {
    this.userService.getUserLeaveBalance(userId).subscribe({
      next: (balance) => {
        this.leaveBalance = balance;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Hiba a szabadság egyenleg betöltése közben';
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

  getProgressPercentage(): number {
    if (!this.leaveBalance) return 0;
    return (this.leaveBalance.usedDays / this.leaveBalance.totalDays) * 100;
  }

  openSettings(): void {
    // TODO: Implement settings modal
    console.log('Open settings');
  }
}

