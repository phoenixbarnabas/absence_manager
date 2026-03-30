import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {WelcomePage} from './components/landing/welcome-page/welcome-page';
import { Profile } from './components/profile/profile';

const routes: Routes = [
  {path:"",redirectTo:"welcome",pathMatch:"full"},
  {path:"welcome",component: WelcomePage},
  {path:"profile",component: Profile},
  {path:"**",redirectTo:"welcome"}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
