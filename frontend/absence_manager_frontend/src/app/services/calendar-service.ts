import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AbsenceRequestViewDto,
  CalendarDayInfoDto,
  CalendarEventDto,
  CalendarEventType,
  CalendarScope,
  CreateAbsenceRequestDto
} from '../models/calendar-models';
import { ConfigService } from './config-service';

@Injectable({
  providedIn: 'root'
})
export class CalendarService {
  constructor(
    private http: HttpClient,
    private configService: ConfigService
  ) { }

  getDayInfos(fromDate: string, toDate: string): Observable<CalendarDayInfoDto[]> {
    const params = new HttpParams()
      .set('fromDate', fromDate)
      .set('toDate', toDate);

    return this.http.get<CalendarDayInfoDto[]>(
      `${this.configService.apiUrl}/calendar/day-infos`,
      { params }
    );
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

    return this.http.get<CalendarEventDto[]>(
      `${this.configService.apiUrl}/calendar/events`,
      { params }
    );
  }

  createAbsenceRequest(dto: CreateAbsenceRequestDto): Observable<AbsenceRequestViewDto> {
    return this.http.post<AbsenceRequestViewDto>(
      `${this.configService.apiUrl}/absence-requests`,
      dto
    );
  }

  cancelAbsenceRequest(id: string): Observable<void> {
    return this.http.delete<void>(
      `${this.configService.apiUrl}/absence-requests/${id}`
    );
  }
}