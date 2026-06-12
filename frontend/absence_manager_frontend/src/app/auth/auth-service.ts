import { Injectable } from '@angular/core';
import { AccountInfo, AuthenticationResult } from '@azure/msal-browser';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { getApiScope, getMsalInstance, getPostLogoutRedirectUri } from './entra-auth-config';

export type AuthProcessState =
  | 'initializing'
  | 'idle'
  | 'loggingIn'
  | 'loggingOut';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly accountSubject = new BehaviorSubject<AccountInfo | null>(null);
  private readonly tokenSubject = new BehaviorSubject<string | null>(null);

  private readonly authProcessStateSubject = new BehaviorSubject<AuthProcessState>('initializing');

  private initialized = false;
  private bootstrapPromise: Promise<void> | null = null;

  readonly account$ = this.accountSubject.asObservable();
  readonly token$ = this.tokenSubject.asObservable();
  readonly authProcessState$ = this.authProcessStateSubject.asObservable();

  async bootstrap(): Promise<void> {
    if (!this.bootstrapPromise) {
      this.bootstrapPromise = this.bootstrapInternal();
    }

    return this.bootstrapPromise;
  }

  private async bootstrapInternal(): Promise<void> {
    this.authProcessStateSubject.next('initializing');

    try {
      await this.initialize();
      await this.handleRedirect();
    } finally {
      if (this.authProcessStateSubject.value === 'initializing') {
        this.authProcessStateSubject.next('idle');
      }
    }
  }

  async initialize(): Promise<void> {
    if (!this.initialized) {
      await getMsalInstance().initialize();
      this.initialized = true;
    }
  }

  async handleRedirect(): Promise<void> {
    const authResult: AuthenticationResult | null =
      await getMsalInstance().handleRedirectPromise();

    const accounts = getMsalInstance().getAllAccounts();

    if (authResult?.account) {
      getMsalInstance().setActiveAccount(authResult.account);
      this.accountSubject.next(authResult.account);
      this.authProcessStateSubject.next('idle');
      return;
    }

    const activeAccount = getMsalInstance().getActiveAccount();

    if (activeAccount) {
      this.accountSubject.next(activeAccount);
      this.authProcessStateSubject.next('idle');
      return;
    }

    if (accounts.length > 0) {
      getMsalInstance().setActiveAccount(accounts[0]);
      this.accountSubject.next(accounts[0]);
      this.authProcessStateSubject.next('idle');
      return;
    }

    this.accountSubject.next(null);
    this.tokenSubject.next(null);
    this.authProcessStateSubject.next('idle');
  }

  async login(): Promise<void> {
    this.authProcessStateSubject.next('loggingIn');

    try {
      await this.initialize();

      await getMsalInstance().loginRedirect({
        scopes: ['openid', 'profile', 'email', getApiScope()]
      });
    } catch (error) {
      this.authProcessStateSubject.next('idle');
      throw error;
    }
  }

  async logout(): Promise<void> {
    this.authProcessStateSubject.next('loggingOut');

    try {
      await this.initialize();

      this.tokenSubject.next(null);
      this.bootstrapPromise = null;

      await getMsalInstance().logoutRedirect({
        postLogoutRedirectUri: getPostLogoutRedirectUri()
      });
    } catch (error) {
      this.authProcessStateSubject.next('idle');
      throw error;
    }
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

    try {
      const result = await getMsalInstance().acquireTokenSilent({
        account,
        scopes: [getApiScope()]
      });

      this.tokenSubject.next(result.accessToken);
      return result.accessToken;
    } catch (error) {
      console.error('Silent token acquisition failed', error);
      this.tokenSubject.next(null);
      return null;
    }
  }

  getAccount(): AccountInfo | null {
    return this.accountSubject.value;
  }

  getToken(): string | null {
    return this.tokenSubject.value;
  }

  getActiveAccount(): AccountInfo | null {
    return getMsalInstance().getActiveAccount()
      ?? getMsalInstance().getAllAccounts()[0]
      ?? null;
  }

  isLoggedIn(): boolean {
    return this.getActiveAccount() !== null || this.getAccount() !== null;
  }
}
