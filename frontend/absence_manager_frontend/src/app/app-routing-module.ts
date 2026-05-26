import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DeskBooking } from './components/desk-booking/desk-booking';
import { Profile } from './components/profile/profile';
import { WelcomePage } from './components/landing/welcome-page/welcome-page';
import { authGuard } from './auth/guards/auth-guard';
import { CalendarPage } from './components/calendar-page/calendar-page';
import { AbsenceApprovalsPage } from './components/absence-approvals-page/absence-approvals-page';
import { MyAbsenceRequestsPage } from './components/my-absence-requests-page/my-absence-requests-page';

const routes: Routes = [
  { path: '', redirectTo: 'welcome', pathMatch: 'full' },
  { path: 'welcome', component: WelcomePage },
  { path: 'login', redirectTo: 'welcome', pathMatch: 'full' },
  { path: 'desk-booking', component: DeskBooking, canActivate: [authGuard] },
  { path: 'calendar', component: CalendarPage, canActivate: [authGuard] },
  { path: 'absence-approvals', component: AbsenceApprovalsPage, canActivate: [authGuard] },
  { path: 'my-absence-requests', component: MyAbsenceRequestsPage, canActivate: [authGuard] },
  { path: 'profile', component: Profile, canActivate: [authGuard] },
  { path: '**', redirectTo: 'welcome' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}