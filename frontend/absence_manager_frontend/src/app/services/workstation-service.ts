import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Workstation } from '../models/entity-models';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from './config-service';

@Injectable({
  providedIn: 'root',
})
export class WorkstationService {
  private apiUrl = environment.apiUrl;

  private workstationSubject = new BehaviorSubject<Workstation[]>([]);
  workstations$ = this.workstationSubject.asObservable();

  constructor(private http: HttpClient, private configService: ConfigService) { }

  loadAllByOfficeId(officeId: string): Observable<Workstation[]> {
    return this.http.get<Workstation[]>(`${this.configService.apiUrl}/workstations/by-office/${officeId}`).pipe(
      tap(workstations => this.workstationSubject.next(workstations))
    );
  }

  clear(): void {
    this.workstationSubject.next([]);
  }
}
