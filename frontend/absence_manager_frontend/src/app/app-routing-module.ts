import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {WelcomePage} from './components/landing/welcome-page/welcome-page';
import { DeskBooking } from './components/desk-booking/desk-booking';

const routes: Routes = [
  {path:"",redirectTo:"welcome",pathMatch:"full"},
  {path:"welcome",component: WelcomePage},
  {path:"desk-booking",component: DeskBooking},
  {path:"**",redirectTo:"welcome"}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
