import { Component, OnInit, signal } from '@angular/core';
import { AuthService } from './auth/auth-service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.sass'
})
export class App implements OnInit {
  protected readonly title = signal('absence_manager_frontend');

  constructor(private authService: AuthService) { }

  async ngOnInit(): Promise<void> {
    try {
      await this.authService.initialize();
      await this.authService.handleRedirect();

      if (this.authService.isLoggedIn()) {
        await this.authService.acquireApiToken();
      }
    } catch (err) {
      console.error('Auth init error', err);
    }
  }
}
