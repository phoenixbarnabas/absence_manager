import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { UserProfile } from '../models/app-user-models';
import { ConfigService } from './config-service';

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

  constructor(private http: HttpClient, private configService: ConfigService) {}

  getMe(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.configService.apiUrl}/users/me`);
  }

  getMeEntra(): Observable<MeResponse> {
    return this.http.get<MeResponse>(`${this.configService.apiUrl}/auth-debug/me`);
  }

  getClaims(): Observable<Array<{ type: string; value: string }>> {
    return this.http.get<Array<{ type: string; value: string }>>(`${this.configService.apiUrl}/auth-debug/claims`);
  }
}