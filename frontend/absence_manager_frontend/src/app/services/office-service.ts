import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';
import { tap } from 'rxjs/internal/operators/tap';
import { Office } from '../models/entity-models';

@Injectable({
  providedIn: 'root',
})
export class OfficeService {
  private apiUrl = environment.apiUrl;
  private officeSubject = new BehaviorSubject<Office[]>([]);
  offices$ = this.officeSubject.asObservable();

  constructor(private http: HttpClient) { }

  loadAllByLocationId(locationId: string): Observable<Office[]> {
    return this.http.get<Office[]>(`${this.apiUrl}/offices/by-location/${locationId}`).pipe(
      tap(office => this.officeSubject.next(office))
    )
  }
}
