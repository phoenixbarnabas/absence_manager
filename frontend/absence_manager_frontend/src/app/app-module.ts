import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import {WelcomePage} from './components/landing/welcome-page/welcome-page';
import { Navbar } from './components/navbar/navbar';
import { FormsModule } from '@angular/forms';
import { WorkstationList } from './components/workstation-list/workstation-list';
import { DeskBooking } from './components/desk-booking/desk-booking';

@NgModule({
  declarations: [
    App,
    WelcomePage,
    Navbar,
    WorkstationList,
    DeskBooking
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
  ],
  bootstrap: [App]
})
export class AppModule { }
