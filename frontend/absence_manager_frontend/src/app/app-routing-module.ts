import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DeskBooking } from './components/desk-booking/desk-booking';
import { Profile } from './components/profile/profile';
import { LoginPage } from './components/login-page/login-page';
import { authGuard } from './auth/guards/auth-guard';

const routes: Routes = [
  { path: '', redirectTo: 'desk-booking', pathMatch: 'full' },

  { path: 'login', component: LoginPage },

  { path: 'desk-booking', component: DeskBooking, canActivate: [authGuard] },
  { path: 'profile', component: Profile, canActivate: [authGuard] },

  { path: '**', redirectTo: 'desk-booking' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}