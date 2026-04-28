import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';
import { tap } from 'rxjs/internal/operators/tap';
import { ConfigService } from './config-service';

export interface DevSeedUser {
  id: string;
  displayName: string;
  email: string;
  entraObjectId: string;
}

export interface DevLoginResponse {
  token: string;
  expiration: string;
  user: {
    id: string;
    displayName: string;
    email: string;
    entraObjectId: string;
    tenantId: string;
    department: string;
    jobTitle: string;
  };
}

@Injectable({
  providedIn: 'root',
})
export class DevAuthService {
  private apiUrl = environment.apiUrl;
  private tokenKey = 'dev-auth-token';
  private userKey = 'dev-auth-user';

  constructor(private http: HttpClient, private configService: ConfigService) { }

  getSeedUsers(): Observable<DevSeedUser[]> {
    return this.http.get<DevSeedUser[]>(`${this.configService.apiUrl}/dev-auth/seed-users`);
  }

  loginAsUser(userId: string): Observable<DevLoginResponse> {
    return this.http.post<DevLoginResponse>(`${this.configService.apiUrl}/dev-auth/login/${userId}`, {}).pipe(
      tap(response => {
        localStorage.setItem(this.tokenKey, response.token);
        localStorage.setItem(this.userKey, JSON.stringify(response.user));
        window.dispatchEvent(new Event('dev-user-changed'));
      })
    );
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getCurrentUser(): DevLoginResponse['user'] | null {
    const raw = localStorage.getItem(this.userKey);
    if (!raw) return null;

    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
  }
}
