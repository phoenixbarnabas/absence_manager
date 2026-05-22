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

  isLoginPage = true;
  authBootstrapping = true;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  get showNavbar(): boolean {
    return !this.isLoginPage && !this.authBootstrapping;
  }

  ngOnInit(): void {
    this.updateRouteState();

    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.updateRouteState();
      });

    void this.initializeAuth();
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

        if (this.isAuthUrl(this.router.url)) {
          await this.router.navigate(['/desk-booking'], {
            replaceUrl: true
          });
        }
      }
    } catch (err) {
      console.error('Auth init error', err);
    } finally {
      this.authBootstrapping = false;
      this.updateRouteState();
    }
  }

  private updateRouteState(): void {
    this.isLoginPage = this.isAuthUrl(this.router.url);
  }

  private isAuthUrl(url: string): boolean {
    return (
      url === '/' ||
      url.startsWith('/welcome') ||
      url.startsWith('/login')
    );
  }
}