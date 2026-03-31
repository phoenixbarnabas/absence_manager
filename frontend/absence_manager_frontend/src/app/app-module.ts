import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, provideHttpClient, withInterceptors } from '@angular/common/http';
import { CommonModule } from '@angular/common';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import {WelcomePage} from './components/landing/welcome-page/welcome-page';
import { Navbar } from './components/navbar/navbar';
import { FormsModule } from '@angular/forms';
import { WorkstationList } from './components/workstation-list/workstation-list';
import { DeskBooking } from './components/desk-booking/desk-booking';
import { Profile } from './components/profile/profile';
import { authInterceptor } from './interceptors/auth-interceptor';

@NgModule({
  declarations: [
    App,
    WelcomePage,
    Navbar,
    Profile,
    Navbar,
    WorkstationList,
    DeskBooking,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule,
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([authInterceptor]))
  ],
  bootstrap: [App]
})
export class AppModule { }
