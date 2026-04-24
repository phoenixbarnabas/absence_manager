import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MeResponse, UserService } from '../../services/user.service';
import { AuthService } from '../../auth/auth-service';

@Component({
  selector: 'app-login-page',
  standalone: false,
  templateUrl: './login-page.html',
  styleUrl: './login-page.sass'
})
export class LoginPage implements OnInit {
  loading = true;
  error: string | null = null;

  me: MeResponse | null = null;
  claims: Array<{ type: string; value: string }> | null = null;

  entraLoading = false;
  entraError: string | null = null;

  constructor(
    private userService: UserService,
    public authService: AuthService,
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      await this.authService.initialize();

      if (this.authService.isLoggedIn()) {
        const loginStarted = sessionStorage.getItem('login_started') === 'true';

        await this.authService.acquireApiToken();

        if (loginStarted) {
          sessionStorage.removeItem('login_started');
          await this.router.navigateByUrl('/desk-booking');
          return;
        }

        this.loadEntraData();
      }
    } catch (error) {
      console.error(error);
      this.entraError = 'Hiba a bejelentkezés vagy a token kezelés közben.';
    } finally {
      this.loading = false;
    }
  }

  async login(): Promise<void> {
    try {
      sessionStorage.setItem('login_started', 'true');
      await this.authService.login();
    } catch (error) {
      console.error(error);
      sessionStorage.removeItem('login_started');
      this.error = 'Nem sikerült elindítani a bejelentkezést.';
    }
  }

  async logout(): Promise<void> {
    sessionStorage.removeItem('login_started');
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