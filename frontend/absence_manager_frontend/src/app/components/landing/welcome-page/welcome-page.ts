import { Component, OnInit } from '@angular/core';
import { MeResponse, UserService } from '../../../services/user.service';
import { UserProfile } from '../../../models/app-user-models';
import { AuthService } from '../../../auth/auth-service';
import { NotificationService } from '../../../services/notification-service';

@Component({
  selector: 'app-welcome-page',
  standalone: false,
  templateUrl: './welcome-page.html',
  styleUrl: './welcome-page.sass',
})
export class WelcomePage implements OnInit {
  userProfile: UserProfile | null = null;
  loading = true;

  me: MeResponse | null = null;
  claims: Array<{ type: string; value: string }> | null = null;

  entraLoading = false;

  constructor(
    private userService: UserService,
    public authService: AuthService,
    private notificationService: NotificationService,
  ) { }

  async ngOnInit(): Promise<void> {
  }

  async login(): Promise<void> {
    try {
      await this.authService.login();
    } catch (err) {
      console.error('Login failed', err);

      this.notificationService.error(
        this.notificationService.getMessage(
          err,
          'Nem sikerült elindítani a bejelentkezést.'
        )
      );
    }
  }

  async logout(): Promise<void> {
    try {
      await this.authService.logout();
    } catch (err) {
      console.error('Logout failed', err);

      this.notificationService.error(
        this.notificationService.getMessage(
          err,
          'Nem sikerült kijelentkezni.'
        )
      );
    }
  }

  private loadEntraData(): void {
    this.entraLoading = true;

    this.loadMe();
    this.loadClaims();
  }

  loadMe(): void {
    this.userService.getMeEntra().subscribe({
      next: res => {
        this.me = res;
        this.finishEntraLoadingIfDone();
      },
      error: err => {
        console.error(err);

        this.notificationService.error(
          this.notificationService.getMessage(
            err,
            'Hiba az Entra adatok betöltése közben.'
          )
        );

        this.finishEntraLoadingIfDone();
      }
    });
  }

  loadClaims(): void {
    this.userService.getClaims().subscribe({
      next: res => {
        this.claims = res;
        this.finishEntraLoadingIfDone();
      },
      error: err => {
        console.error(err);

        this.notificationService.error(
          this.notificationService.getMessage(
            err,
            'Hiba a claim-ek betöltése közben.'
          )
        );

        this.finishEntraLoadingIfDone();
      }
    });
  }

  private finishEntraLoadingIfDone(): void {
    if (this.me !== null || this.claims !== null) {
      this.entraLoading = false;
    }
  }
}
