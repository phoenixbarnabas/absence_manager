import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {WelcomePage} from './components/landing/welcome-page/welcome-page';
import { DeskBooking } from './components/desk-booking/desk-booking';
import { Profile } from './components/profile/profile';
import { MsalGuard } from '@azure/msal-angular';

const routes: Routes = [
  {path:"",redirectTo:"login",pathMatch:"full"},
  {path:"welcome",component: WelcomePage},
  {path:"desk-booking",component: DeskBooking, canActivate: [MsalGuard]},
  {path:"profile",component: Profile, canActivate: [MsalGuard]},
  {path:"**",redirectTo:"login"}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
