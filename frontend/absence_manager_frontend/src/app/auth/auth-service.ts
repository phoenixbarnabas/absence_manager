import { Injectable } from '@angular/core';
import { authState } from './auth.state';
import { apiScope, msalInstance, postLogoutRedirectUri } from './entra-auth-config';
import { AccountInfo, AuthenticationResult } from '@azure/msal-browser';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly accountSubject = new BehaviorSubject<AccountInfo | null>(null);
  private readonly tokenSubject = new BehaviorSubject<string | null>(null);
  private initialized = false;

  readonly account$ = this.accountSubject.asObservable();
  readonly token$ = this.tokenSubject.asObservable();

  async initialize(): Promise<void> {
    if (!this.initialized) {
      await msalInstance.initialize();
      this.initialized = true;
    }
  }

  async handleRedirect(): Promise<void> {
    await this.initialize();

    const authResult: AuthenticationResult | null = await msalInstance.handleRedirectPromise();
    const accounts = msalInstance.getAllAccounts();

    if (authResult?.account) {
      msalInstance.setActiveAccount(authResult.account);
      this.accountSubject.next(authResult.account);
    } else if (accounts.length > 0) {
      msalInstance.setActiveAccount(accounts[0]);
      this.accountSubject.next(accounts[0]);
    } else {
      this.accountSubject.next(null);
      this.tokenSubject.next(null);
    }
  }

  async login(): Promise<void> {
    await this.initialize();

    await msalInstance.loginRedirect({
      scopes: ['openid', 'profile', 'email', apiScope]
    });
  }

  async logout(): Promise<void> {
    await this.initialize();

    this.accountSubject.next(null);
    this.tokenSubject.next(null);

    await msalInstance.logoutRedirect({
      postLogoutRedirectUri
    });
  }

  async acquireApiToken(): Promise<string | null> {
    await this.initialize();

    const existingToken = this.tokenSubject.value;
    if (existingToken) {
      return existingToken;
    }

    const account = this.getActiveAccount();
    if (!account) {
      this.tokenSubject.next(null);
      return null;
    }

    const result = await msalInstance.acquireTokenSilent({
      account,
      scopes: [apiScope]
    });

    this.tokenSubject.next(result.accessToken);
    return result.accessToken;
  }

  getAccount(): AccountInfo | null {
    return this.accountSubject.value;
  }

  getToken(): string | null {
    return this.tokenSubject.value;
  }

  getActiveAccount(): AccountInfo | null {
    return msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0] ?? null;
  }

  isLoggedIn(): boolean {
    return this.getActiveAccount() !== null || this.getAccount() !== null;
  }
}
