import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { UserProfile } from '../models/app-user-models';

export type MeResponse = {
  oid: string | null;
  tid: string | null;
  name: string | null;
  preferred_username: string | null;
  email: string | null;
  upn: string | null;
  scp: string | null;
  roles: string[];
};

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly usersApiUrl = `${environment.apiUrl}/users`;
  private readonly authDebugApiUrl = `${environment.apiUrl}/auth-debug`;

  constructor(private http: HttpClient) {}

  getMe(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.usersApiUrl}/me`);
  }

  getMeEntra(): Observable<MeResponse> {
    return this.http.get<MeResponse>(`${this.authDebugApiUrl}/me`);
  }

  getClaims(): Observable<Array<{ type: string; value: string }>> {
    return this.http.get<Array<{ type: string; value: string }>>(`${this.authDebugApiUrl}/claims`);
  }
}