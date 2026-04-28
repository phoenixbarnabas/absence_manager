import { APP_INITIALIZER, NgModule } from '@angular/core';
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
import { WelcomePage } from './components/landing/welcome-page/welcome-page';
import { WorkstationList } from './components/workstation-list/workstation-list';
import { authInterceptor } from './auth/auth-interceptor';
import { ConfigService } from './services/config-service';

@NgModule({
  declarations: [
    App,
    Navbar,
    Profile,
    DeskBooking,
    WelcomePage,
    WorkstationList
  ],
  imports: [
    FormsModule,
    BrowserModule,
    AppRoutingModule,
  ],
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
    {
      provide: APP_INITIALIZER,
      useFactory: (cfg: ConfigService) => () => cfg.loadConfig(),
      deps: [ConfigService],
      multi: true,
    },
  ],
  bootstrap: [App]
})
export class AppModule { }