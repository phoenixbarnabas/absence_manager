import { BrowserCacheLocation, Configuration, LogLevel, PublicClientApplication } from '@azure/msal-browser';

const tenantId = '1878a48b-63d6-4d12-a900-07d4267f6762';
const clientId = 'cacb868f-e5d8-4113-acde-780f810c824d';

export const apiScope = 'api://cacb868f-e5d8-4113-acde-780f810c824d/user_impersonation';
export const postLogoutRedirectUri = 'http://localhost:4200/welcome';

export const msalConfig: Configuration = {
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri: 'http://localhost:4200/welcome',
    postLogoutRedirectUri
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

export const msalInstance = new PublicClientApplication(msalConfig);