import { Component, OnInit } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { MeResponse, UserService } from '../../../services/user.service';
import { AuthenticationResult } from '@azure/msal-browser';
import { UserProfile } from '../../../models/app-user-models';
import { AuthService } from '../../../auth/auth-service';

@Component({
  selector: 'app-welcome-page',
  standalone: false,
  templateUrl: './welcome-page.html',
  styleUrl: './welcome-page.sass',
})
export class WelcomePage implements OnInit {
userProfile: UserProfile | null = null;
  loading = true;
  error: string | null = null;

  me: MeResponse | null = null;
  claims: Array<{ type: string; value: string }> | null = null;

  entraLoading = false;
  entraError: string | null = null;

  constructor(
    private userService: UserService,
    public authService: AuthService
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      await this.authService.initialize();
      await this.authService.handleRedirect();

      if (this.authService.isLoggedIn()) {
        await this.authService.acquireApiToken();
        this.loadEntraData();
      }
    } catch (error) {
      console.error(error);
      this.entraError = 'Hiba a bejelentkezés vagy a token kezelés közben.';
    }
  }

  async login(): Promise<void> {
    await this.authService.login()
  }

  async logout(): Promise<void> {
    await this.authService.logout();
  }

  private loadEntraData(): void {
    this.entraLoading = true;
    this.entraError = null;

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
        this.entraError = 'Hiba az Entra adatok betöltése közben.';
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
        this.entraError = 'Hiba a claim-ek betöltése közben.';
        this.finishEntraLoadingIfDone();
      }
    });
  }

  private finishEntraLoadingIfDone(): void {
    if (this.me !== null || this.claims !== null || this.entraError !== null) {
      this.entraLoading = false;
    }
  }
}
