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
  private graphProfileSyncStarted = false;

  constructor(
    private router: Router,
    private authService: AuthService,
    private userService: UserService
  ) {}

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
          this.graphProfileSyncStarted = false;
          return;
        }

        this.startGraphProfileSync();
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

      if (this.authService.isLoggedIn()) {
        this.startGraphProfileSync();
      }
    } catch (error) {
      console.error('Auth bootstrap failed in App.', error);
    }
  }

  private startGraphProfileSync(): void {
    if (this.graphProfileSyncStarted) {
      return;
    }

    this.graphProfileSyncStarted = true;

    this.userService.syncGraphProfile()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        error: err => {
          console.warn('Graph profil szinkron sikertelen.', err);
        }
      });
  }

  private updateRouteState(): void {
    this.isLoginPage =
      this.router.url.startsWith('/welcome') ||
      this.router.url.startsWith('/login');
  }
}