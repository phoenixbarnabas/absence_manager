import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../auth/auth-service';
import { MeResponse, UserService } from '../../services/user.service';

@Component({
  selector: 'app-login-page',
  standalone: false,
  templateUrl: './login-page.html',
  styleUrl: './login-page.sass'
})
export class LoginPage implements OnInit {
  loading = true;
  loginInProgress = false;
  error: string | null = null;

  me: MeResponse | null = null;
  claims: Array<{ type: string; value: string }> | null = null;

  entraLoading = false;
  entraError: string | null = null;

  constructor(
    public authService: AuthService,
    private userService: UserService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      await this.authService.initialize();
      await this.authService.handleRedirect();

      if (this.authService.isLoggedIn()) {
        await this.authService.acquireApiToken();

        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');

        if (returnUrl) {
          await this.router.navigateByUrl(returnUrl);
          return;
        }

        this.loadEntraData();
      }
    } catch (error) {
      console.error(error);
      this.error = 'Nem sikerült inicializálni a bejelentkezést.';
    } finally {
      this.loading = false;
    }
  }

  async login(): Promise<void> {
    this.loginInProgress = true;
    this.error = null;

    try {
      await this.authService.login();
    } catch (error) {
      console.error(error);
      this.error = 'A bejelentkezés indítása sikertelen.';
      this.loginInProgress = false;
    }
  }

  async logout(): Promise<void> {
    await this.authService.logout();
  }

  async continueToApp(): Promise<void> {
    await this.router.navigateByUrl('/desk-booking');
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