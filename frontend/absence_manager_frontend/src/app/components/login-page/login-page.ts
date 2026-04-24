import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { SessionService } from '../../services/session-service';

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

  constructor(
    private sessionService: SessionService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      await this.sessionService.init();

      if (this.sessionService.isLoggedIn) {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/desk-booking';
        await this.router.navigateByUrl(returnUrl);
        return;
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
      await this.sessionService.login();
    } catch (error) {
      console.error(error);
      this.error = 'A bejelentkezés indítása sikertelen.';
      this.loginInProgress = false;
    }
  }
}