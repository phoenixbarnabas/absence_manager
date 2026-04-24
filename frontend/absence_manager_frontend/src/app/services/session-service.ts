/* import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AccountInfo, AuthenticationResult } from '@azure/msal-browser';
import { apiScope, msalInstance, postLogoutRedirectUri } from '../auth/entra-auth-config';

@Injectable({
  providedIn: 'root'
})
export class SessionService {
  private initialized = false;
  private initPromise: Promise<void> | null = null;
  private accessToken: string | null = null;

  private readonly accountSubject = new BehaviorSubject<AccountInfo | null>(null);
  private readonly readySubject = new BehaviorSubject<boolean>(false);

  readonly account$ = this.accountSubject.asObservable();
  readonly ready$ = this.readySubject.asObservable();

  async init(): Promise<void> {
    if (this.initialized && this.readySubject.value) {
      return;
    }

    if (this.initPromise) {
      return this.initPromise;
    }

    this.initPromise = this.doInit();

    try {
      await this.initPromise;
    } finally {
      this.initPromise = null;
    }
  }

  private async doInit(): Promise<void> {
    if (!this.initialized) {
      await msalInstance.initialize();
      this.initialized = true;
    }

    try {
      const result: AuthenticationResult | null = await msalInstance.handleRedirectPromise();

      if (result?.account) {
        msalInstance.setActiveAccount(result.account);
      }

      const active =
        msalInstance.getActiveAccount() ??
        msalInstance.getAllAccounts()[0] ??
        null;

      if (active) {
        msalInstance.setActiveAccount(active);
      }

      this.accountSubject.next(active);

      if (!active) {
        this.accessToken = null;
      }
    } catch (error) {
      console.error('Session init failed', error);
      this.accountSubject.next(null);
      this.accessToken = null;
      throw error;
    } finally {
      this.readySubject.next(true);
    }
  }

  get account(): AccountInfo | null {
    return this.accountSubject.value;
  }

  get isLoggedIn(): boolean {
    return !!this.accountSubject.value;
  }

  get token(): string | null {
    return this.accessToken;
  }

  async login(): Promise<void> {
    await this.init();

    await msalInstance.loginRedirect({
      scopes: ['openid', 'profile', 'email', apiScope]
    });
  }

  async logout(): Promise<void> {
    this.accountSubject.next(null);
    this.accessToken = null;
    this.readySubject.next(false);

    await msalInstance.logoutRedirect({
      postLogoutRedirectUri
    });
  }

  async getAccessToken(): Promise<string | null> {
    await this.init();

    const account = this.account;
    if (!account) {
      this.accessToken = null;
      return null;
    }

    try {
      const result = await msalInstance.acquireTokenSilent({
        account,
        scopes: [apiScope]
      });

      this.accessToken = result.accessToken;
      return result.accessToken;
    } catch (error) {
      console.error('Silent token acquisition failed', error);
      this.accessToken = null;
      return null;
    }
  }
} */