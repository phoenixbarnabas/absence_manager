import { Injectable } from '@angular/core';
import { AccountInfo } from '@azure/msal-browser';
import { SessionService } from '../services/session-service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private sessionService: SessionService) {}

  get account$() {
    return this.sessionService.account$;
  }

  get ready$() {
    return this.sessionService.ready$;
  }

  async initialize(): Promise<void> {
    await this.sessionService.init();
  }

  async login(): Promise<void> {
    await this.sessionService.login();
  }

  async logout(): Promise<void> {
    await this.sessionService.logout();
  }

  async acquireApiToken(): Promise<string | null> {
    return this.sessionService.getAccessToken();
  }

  getAccount(): AccountInfo | null {
    return this.sessionService.account;
  }

  getActiveAccount(): AccountInfo | null {
    return this.sessionService.account;
  }

  getToken(): string | null {
    return this.sessionService.token;
  }

  isLoggedIn(): boolean {
    return this.sessionService.isLoggedIn;
  }
}