import { BrowserCacheLocation, Configuration, LogLevel, PublicClientApplication } from '@azure/msal-browser';
import { AppConfig } from '../models/app-config.model';

let msalInstance: PublicClientApplication | null = null;
let appConfig: AppConfig | null = null;

export function buildMsalConfig(config: AppConfig): Configuration {
  return {
    auth: {
      clientId: config.azureClientId,
      authority: `https://login.microsoftonline.com/${config.azureTenantId}`,
      redirectUri: config.postLogoutRedirectUri,
      postLogoutRedirectUri: config.postLogoutRedirectUri
    },
    cache: {
      cacheLocation: BrowserCacheLocation.LocalStorage
    },
    system: {
      loggerOptions: {
        loggerCallback: (_level: LogLevel, _message: string) => {},
        logLevel: LogLevel.Info,
        piiLoggingEnabled: false
      }
    }
  };
}

export function createMsalInstance(config: AppConfig): PublicClientApplication {
  appConfig = config;
  msalInstance = new PublicClientApplication(buildMsalConfig(config));
  return msalInstance;
}

export function getMsalInstance(): PublicClientApplication {
  if (!msalInstance) {
    throw new Error('MSAL instance is not initialized yet.');
  }

  return msalInstance;
}

export function getApiScope(): string {
  if (!appConfig) {
    throw new Error('App config is not initialized yet.');
  }

  return appConfig.apiScope;
}

export function getPostLogoutRedirectUri(): string {
  if (!appConfig) {
    throw new Error('App config is not initialized yet.');
  }

  return appConfig.postLogoutRedirectUri;
}