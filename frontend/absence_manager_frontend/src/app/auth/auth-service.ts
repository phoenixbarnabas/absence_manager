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
  private tokenRequestPromise: Promise<string | null> | null = null;

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
    const msal = getMsalInstance();

    const authResult: AuthenticationResult | null =
      await msal.handleRedirectPromise();

    const account =
      authResult?.account ??
      msal.getActiveAccount() ??
      msal.getAllAccounts()[0] ??
      null;

    if (account) {
      msal.setActiveAccount(account);
    }

    this.accountSubject.next(account);
    this.tokenSubject.next(null);
    this.authProcessStateSubject.next('idle');
  }

  async login(): Promise<void> {
    this.authProcessStateSubject.next('loggingIn');

    try {
      await this.initialize();

      this.tokenSubject.next(null);

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
      this.accountSubject.next(null);
      this.bootstrapPromise = null;
      this.tokenRequestPromise = null;

      await getMsalInstance().logoutRedirect({
        account: this.getActiveAccount() ?? undefined,
        postLogoutRedirectUri: getPostLogoutRedirectUri()
      });
    } catch (error) {
      this.authProcessStateSubject.next('idle');
      throw error;
    }
  }

  async acquireApiToken(): Promise<string | null> {
    await this.initialize();

    if (this.tokenRequestPromise) {
      return this.tokenRequestPromise;
    }

    this.tokenRequestPromise = this.acquireApiTokenInternal()
      .finally(() => {
        this.tokenRequestPromise = null;
      });

    return this.tokenRequestPromise;
  }

  private async acquireApiTokenInternal(): Promise<string | null> {
    const account = this.getActiveAccount();

    if (!account) {
      this.tokenSubject.next(null);
      this.accountSubject.next(null);
      return null;
    }

    try {
      const result = await getMsalInstance().acquireTokenSilent({
        account,
        scopes: [getApiScope()]
      });

      if (result.account) {
        getMsalInstance().setActiveAccount(result.account);
        this.accountSubject.next(result.account);
      } else {
        this.accountSubject.next(account);
      }

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
    const msal = getMsalInstance();

    const account =
      msal.getActiveAccount() ??
      msal.getAllAccounts()[0] ??
      null;

    if (account && !msal.getActiveAccount()) {
      msal.setActiveAccount(account);
    }

    return account;
  }

  isLoggedIn(): boolean {
    return this.getActiveAccount() !== null || this.getAccount() !== null;
  }
}
