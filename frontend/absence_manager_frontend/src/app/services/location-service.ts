import { HttpBackend, HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Location } from '../models/entity-models';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root',
})
export class LocationService {
  private apiUrl = environment.apiUrl;
  private locationSubject = new BehaviorSubject<Location[]>([]);
  location$ = this.locationSubject.asObservable();

  constructor(private http: HttpClient) {}

  loadAll(): Observable<Location[]> {
    return this.http.get<Location[]>(`${this.apiUrl}/locations`).pipe(
      tap(location => this.locationSubject.next(location))
    )
  }
}
