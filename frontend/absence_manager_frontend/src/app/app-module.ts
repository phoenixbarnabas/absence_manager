import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { MsalModule } from '@azure/msal-angular';
import { FormsModule } from '@angular/forms';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { InteractionType } from '@azure/msal-browser';

import { App } from './app';
import { AppRoutingModule } from './app-routing-module';
import { Navbar } from './components/navbar/navbar';
import { Profile } from './components/profile/profile';
import { DeskBooking } from './components/desk-booking/desk-booking';
import { LoginPage } from './components/login-page/login-page';
import { WelcomePage } from './components/landing/welcome-page/welcome-page';
import { WorkstationList } from './components/workstation-list/workstation-list';
import { authInterceptor } from './auth/auth-interceptor';
import { msalInstance } from './auth/entra-auth-config';

@NgModule({
  declarations: [
    App,
    Navbar,
    Profile,
    DeskBooking,
    LoginPage,
    WelcomePage,
    WorkstationList
  ],
  imports: [
    FormsModule,
    BrowserModule,
    AppRoutingModule,
    MsalModule.forRoot(
      msalInstance,
      {
        interactionType: InteractionType.Redirect,
        authRequest: {
          scopes: [
            'openid',
            'profile',
            'email',
            'api://cacb868f-e5d8-4113-acde-780f810c824d/user_impersonation'
          ]
        }
      },
      {
        interactionType: InteractionType.Redirect,
        protectedResourceMap: new Map()
      }
    ),
  ],
  providers: [
    provideHttpClient(withInterceptors([authInterceptor]))
  ],
  bootstrap: [App]
})
export class AppModule { }