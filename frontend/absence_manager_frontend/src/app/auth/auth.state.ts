import { AccountInfo } from "@azure/msal-browser";

export type AuthState = {
  account: AccountInfo | null;
  token: string | null;
  initialized: boolean;
}

export const authState: AuthState = {
  account: null,
  token: null,
  initialized: false
}