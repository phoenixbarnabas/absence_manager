import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserProfile {
  id: string;
  displayName: string;
  email: string;
  department: string;
  jobTitle: string;
}

export interface LeaveBalance {
  totalDays: number;
  usedDays: number;
  sickLeaveDays: number;
  pendingDays: number;
  remainingDays: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = 'http://localhost:5000/api/users';

  constructor(private http: HttpClient) { }

  getUserProfile(userId: string): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/${userId}/profile`);
  }

  getUserLeaveBalance(userId: string): Observable<LeaveBalance> {
    return this.http.get<LeaveBalance>(`${this.apiUrl}/${userId}/leave-balance`);
  }
}
