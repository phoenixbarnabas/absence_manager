import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Subject, filter, takeUntil } from 'rxjs';
import { AuthService } from './auth/auth-service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.sass'
})
export class App implements OnInit, OnDestroy {
  protected readonly title = signal('absence_manager_frontend');
  isLoginPage = false;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  async ngOnInit(): Promise<void> {
    await this.initializeAuth();
    this.updateRouteState();

    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => this.updateRouteState());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private async initializeAuth(): Promise<void> {
    try {
      await this.authService.initialize();
      await this.authService.handleRedirect();

      if (this.authService.isLoggedIn()) {
        await this.authService.acquireApiToken();

        if (this.router.url === '/' || this.router.url.startsWith('/welcome') || this.router.url.startsWith('/login')) {
          await this.router.navigate(['/desk-booking']);
        }
      }
    } catch (err) {
      console.error('Auth init error', err);
    }
  }

  private updateRouteState(): void {
    this.isLoginPage = this.router.url.startsWith('/welcome') || this.router.url.startsWith('/login');
  }
}