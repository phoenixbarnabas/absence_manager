import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import {WelcomePage} from './components/landing/welcome-page/welcome-page';
import { Navbar } from './components/navbar/navbar';
import { Profile } from './components/profile/profile';

@NgModule({
  declarations: [
    App,
    WelcomePage,
    Navbar,
    Profile
  ],
  imports: [
    BrowserModule,
    CommonModule,
    HttpClientModule,
    AppRoutingModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
  ],
  bootstrap: [App]
})
export class AppModule { }
