import { Injectable } from '@angular/core';
import { AccountInfo, AuthenticationResult } from '@azure/msal-browser';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { getApiScope, getMsalInstance, getPostLogoutRedirectUri } from './entra-auth-config';

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
      await getMsalInstance().initialize();
      this.initialized = true;
    }
  }

  async handleRedirect(): Promise<void> {
    await this.initialize();

    const authResult: AuthenticationResult | null = await getMsalInstance().handleRedirectPromise();
    const accounts = getMsalInstance().getAllAccounts();

    if (authResult?.account) {
      getMsalInstance().setActiveAccount(authResult.account);
      this.accountSubject.next(authResult.account);
    } else if (accounts.length > 0) {
      getMsalInstance().setActiveAccount(accounts[0]);
      this.accountSubject.next(accounts[0]);
    } else {
      this.accountSubject.next(null);
      this.tokenSubject.next(null);
    }
  }

  async login(): Promise<void> {
    await this.initialize();

    await getMsalInstance().loginRedirect({
      scopes: ['openid', 'profile', 'email', getApiScope()]
    });
  }

  async logout(): Promise<void> {
    await this.initialize();

    this.accountSubject.next(null);
    this.tokenSubject.next(null);

    await getMsalInstance().logoutRedirect({
      postLogoutRedirectUri: getPostLogoutRedirectUri()
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

    const result = await getMsalInstance().acquireTokenSilent({
      account,
      scopes: [getApiScope()]
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
    return getMsalInstance().getActiveAccount() ?? getMsalInstance().getAllAccounts()[0] ?? null;
  }

  isLoggedIn(): boolean {
    return this.getActiveAccount() !== null || this.getAccount() !== null;
  }
}
