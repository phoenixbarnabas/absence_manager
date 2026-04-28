import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { OfficeDayAvailabilityDto } from '../models/availability-models';
import { CreateOfficeBookingDto, DaySummaryDto, OfficeBookingViewDto } from '../models/booking-models';
import { ConfigService } from './config-service';

@Injectable({
  providedIn: 'root',
})
export class BookingService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private configService: ConfigService) { }

  getAvailability(officeId: string, date: string): Observable<OfficeDayAvailabilityDto>{
    const params = new HttpParams()
      .set('officeId', officeId)
      .set('date', date);
    return this.http.get<OfficeDayAvailabilityDto>(`${this.configService.apiUrl}/office-bookings/availability`, { params })
  }

  getDaySummaries(officeId: string, fromDate: string, toDate: string): Observable<DaySummaryDto[]> {
    const params = new HttpParams()
      .set('officeId', officeId)
      .set('fromDate', fromDate)
      .set('toDate', toDate);
    return this.http.get<DaySummaryDto[]>(`${this.configService.apiUrl}/office-bookings/day-summaries`, { params });
  }

  getMyBookings(fromDate: string, toDate: string): Observable<OfficeBookingViewDto[]> {
    const params = new HttpParams()
      .set('fromDate', fromDate)
      .set('toDate', toDate);
    return this.http.get<OfficeBookingViewDto[]>(`${this.configService.apiUrl}/office-bookings/my`, { params });
  }

  createBooking(workstationId: string, bookingDate: string): Observable<CreateOfficeBookingDto> {
    const body = { workstationId, bookingDate };
    return this.http.post<CreateOfficeBookingDto>(`${this.configService.apiUrl}/office-bookings`, body);
  }

  cancelBooking(bookingId: string): Observable<void> {
    return this.http.delete<void>(`${this.configService.apiUrl}/office-bookings/${bookingId}`);
  }
}
