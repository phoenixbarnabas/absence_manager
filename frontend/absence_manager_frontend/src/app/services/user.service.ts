import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { UserProfile } from '../models/app-user-models';



@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getMe(): Observable<UserProfile> {
    console.log('GET ME CALLED');
    return this.http.get<UserProfile>(`${this.apiUrl}/me`);
  }
}