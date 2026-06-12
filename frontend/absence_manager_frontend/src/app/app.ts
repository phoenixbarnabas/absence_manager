import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { Subject, distinctUntilChanged, filter, takeUntil } from 'rxjs';
import { AuthService } from './auth/auth-service';
import { UserService } from './services/user.service';

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
  private graphSyncStarted = false;

  constructor(
    private router: Router,
    private authService: AuthService,
    private userService: UserService
  ) { }

  async ngOnInit(): Promise<void> {
    await this.bootstrapAuth();

    this.authService.account$
      .pipe(
        distinctUntilChanged((previous, current) =>
          previous?.homeAccountId === current?.homeAccountId
        ),
        takeUntil(this.destroy$)
      )
      .subscribe(account => {
        if (!account) {
          this.graphSyncStarted = false;
          return;
        }

        void this.startGraphSyncIfTokenAvailable();
      });

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

  private async bootstrapAuth(): Promise<void> {
    try {
      await this.authService.bootstrap();
      await this.startGraphSyncIfTokenAvailable();
    } catch (error) {
      console.error('Auth bootstrap failed in App.', error);
    }
  }

  private async startGraphSyncIfTokenAvailable(): Promise<void> {
    if (this.graphSyncStarted) {
      return;
    }

    const token = await this.authService.acquireApiToken();

    if (!token) {
      this.graphSyncStarted = false;
      return;
    }

    this.startGraphSync();
  }

  private startGraphSync(): void {
    if (this.graphSyncStarted) {
      return;
    }

    this.graphSyncStarted = true;

    this.userService.syncCurrentUserFromGraph()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        error: err => {
          console.warn('Graph szinkron sikertelen.', err);
          this.graphSyncStarted = false;
        }
      });
  }

  private updateRouteState(): void {
    this.isLoginPage =
      this.router.url.startsWith('/welcome') ||
      this.router.url.startsWith('/login');
  }
}