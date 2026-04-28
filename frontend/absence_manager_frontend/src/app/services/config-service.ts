import { HttpBackend, HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom, tap } from 'rxjs';
import { AppConfig } from '../models/app-config.model';
import { createMsalInstance } from '../auth/entra-auth-config';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private config: AppConfig | null = null;
  private rawHttp: HttpClient;

  constructor(httpBackend: HttpBackend) {
    this.rawHttp = new HttpClient(httpBackend);
  }

  loadConfig(): Promise<void> {
    return firstValueFrom(this.rawHttp.get<AppConfig>('/config.json'))
      .then((config) => {
        this.config = config;
        createMsalInstance(config);
      });
  }

  get isLoaded(): boolean {
    return this.config !== null;
  }

  get apiUrl(): string {
    return this.getConfig().apiUrl;
  }

  get azureClientId(): string {
    return this.getConfig().azureClientId;
  }

  get azureTenantId(): string {
    return this.getConfig().azureTenantId;
  }

  get apiScope(): string {
    return this.getConfig().apiScope;
  }

  get postLogoutRedirectUri(): string {
    return this.getConfig().postLogoutRedirectUri;
  }

  private getConfig(): AppConfig {
    if (!this.config) {
      throw new Error('Config is not loaded yet.');
    }

    return this.config;
  }
}
