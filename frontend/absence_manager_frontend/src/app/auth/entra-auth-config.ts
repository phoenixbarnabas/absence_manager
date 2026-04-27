import { BrowserCacheLocation, Configuration, LogLevel, PublicClientApplication } from '@azure/msal-browser';
import { environment } from '../../environments/environment.development';

const tenantId = environment.tenantId;
const clientId = environment.clientId;

export const apiScope = environment.apiScope;
export const postLogoutRedirectUri = environment.postLogoutRedirectUri;

export const msalConfig: Configuration = {
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri: postLogoutRedirectUri,
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