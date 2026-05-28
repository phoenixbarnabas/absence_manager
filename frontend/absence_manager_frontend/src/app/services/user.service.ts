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

export interface GraphAppUserDto {
  entraObjectId: string;
  appUserId?: string | null;
  displayName?: string | null;
  email?: string | null;
  userPrincipalName?: string | null;
  department?: string | null;
  jobTitle?: string | null;
  officeLocation?: string | null;
  isKnownLocalUser: boolean;
  isActiveLocalUser: boolean;
}

export interface AppUserHierarchyDto {
  currentUser?: GraphAppUserDto | null;
  manager?: GraphAppUserDto | null;
  directReports: GraphAppUserDto[];
}

export interface UserContextDto {
  profile: UserProfile;
  hierarchy: AppUserHierarchyDto;
  isManager: boolean;
  roles: string[];
  lastGraphSyncAtUtc?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(
    private http: HttpClient,
    private configService: ConfigService
  ) { }

  getMe(): Observable<UserProfile> {
    return this.http.get<UserProfile>(
      `${this.configService.apiUrl}/users/me`
    );
  }

  syncCurrentUserFromGraph(): Observable<UserContextDto> {
    return this.http.post<UserContextDto>(
      `${this.configService.apiUrl}/users/me/sync-from-graph`,
      {}
    );
  }

  getMeEntra(): Observable<MeResponse> {
    return this.http.get<MeResponse>(
      `${this.configService.apiUrl}/auth-debug/me`
    );
  }

  getClaims(): Observable<Array<{ type: string; value: string }>> {
    return this.http.get<Array<{ type: string; value: string }>>(
      `${this.configService.apiUrl}/auth-debug/claims`
    );
  }
}