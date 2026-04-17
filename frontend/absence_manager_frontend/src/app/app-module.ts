import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { MsalGuard, MsalModule } from '@azure/msal-angular';
import { Navbar } from './components/navbar/navbar';
import { App } from './app';
import { WelcomePage } from './components/landing/welcome-page/welcome-page';
import { Profile } from './components/profile/profile';
import { WorkstationList } from './components/workstation-list/workstation-list';
import { DeskBooking } from './components/desk-booking/desk-booking';
import { AppRoutingModule } from './app-routing-module';
import { FormsModule } from '@angular/forms';
import { authInterceptor } from './auth/auth-interceptor';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { InteractionType } from '@azure/msal-browser';
import { msalInstance } from './auth/entra-auth-config';

@NgModule({
  declarations: [
    App,
    WelcomePage,
    Profile,
    Navbar,
    WorkstationList,
    DeskBooking,
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
          scopes: ['openid', 'profile', 'email', 'api://cacb868f-e5d8-4113-acde-780f810c824d/user_impersonation']
        },
        loginFailedRoute: '/login-failed'
      },
      {
        interactionType: InteractionType.Redirect,
        protectedResourceMap: new Map()
      }
    ),
  ],
  providers: [
    MsalGuard,
    provideHttpClient(
      withInterceptors([authInterceptor])
    )
  ],
  bootstrap: [App]
})
export class AppModule { }

