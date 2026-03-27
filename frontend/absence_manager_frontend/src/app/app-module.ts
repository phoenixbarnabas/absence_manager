import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import {WelcomePage} from './components/landing/welcome-page/welcome-page';
import { Navbar } from './components/navbar/navbar';

@NgModule({
  declarations: [
    App,
    WelcomePage,
    Navbar
  ],
  imports: [
    BrowserModule,
    AppRoutingModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
  ],
  bootstrap: [App]
})
export class AppModule { }
