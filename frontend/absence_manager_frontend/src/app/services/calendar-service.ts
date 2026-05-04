import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { CalendarDayInfoDto, CalendarEventDto, CalendarEventType, CalendarScope } from '../models/calendar-models';

@Injectable({
  providedIn: 'root'
})
export class CalendarService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getDayInfos(fromDate: string, toDate: string): Observable<CalendarDayInfoDto[]> {
    const params = new HttpParams()
      .set('fromDate', fromDate)
      .set('toDate', toDate);

    return this.http.get<CalendarDayInfoDto[]>(`${this.apiUrl}/calendar/day-infos`, { params });
  }

  getEvents(
    fromDate: string,
    toDate: string,
    scope: CalendarScope,
    eventTypes: CalendarEventType[]
  ): Observable<CalendarEventDto[]> {
    let params = new HttpParams()
      .set('fromDate', fromDate)
      .set('toDate', toDate)
      .set('scope', scope);

    eventTypes.forEach(type => {
      params = params.append('eventTypes', type);
    });

    return this.http.get<CalendarEventDto[]>(`${this.apiUrl}/calendar/events`, { params });
  }
}
