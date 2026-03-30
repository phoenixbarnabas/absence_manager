import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LocationService } from '../../services/location-service';
import { Observable, tap } from 'rxjs';
import { Location, Office } from '../../models/entity-models';
import { OfficeService } from '../../services/office-service';

type DeskStatus = 'free' | 'selected' | 'occupied';

type CalendarDay = {
  date: Date;
  dayLabel: string;
  dayNumber: number;
  isSelected: boolean;
}

@Component({
  selector: 'app-desk-booking',
  standalone: false,
  templateUrl: './desk-booking.html',
  styleUrl: './desk-booking.sass',
})
export class DeskBooking implements OnInit {
  calendarDays: CalendarDay[] = []

  locations$!: Observable<Location[]>
  offices$!: Observable<Office[]>

  selectedLocationId!: string
  selectedOfficeId!: string

  constructor(
    private locationService: LocationService,
    private officeService: OfficeService
  ) { }

  ngOnInit(): void {
    this.generateCalendarDays()
    this.loadLocations()
  }

  loadLocations(): void {
    this.locationService.loadAll().subscribe()
    this.locations$ = this.locationService.location$.pipe(tap(locations => {
      if (!this.selectedLocationId && locations.length > 0) {
        this.selectedLocationId = locations[0].id
        this.loadOffices(this.selectedLocationId)
      }
    }))
  }

  loadOffices(locationId: string): void {
    if (!locationId) {
      return;
    }

    this.selectedOfficeId = '';

    this.officeService.loadAllByLocationId(locationId).subscribe();

    this.offices$ = this.officeService.offices$.pipe(
      tap(offices => {
        if (!this.selectedOfficeId && offices.length > 0) {
          this.selectedOfficeId = offices[0].id;
        }
      })
    )
  }

  currentLocation$(locations: Location[]): Location | undefined {
    return locations.find(l => l.id === this.selectedLocationId);
  }

  currentOffice$(offices: Office[]): Office | undefined {
    return offices.find(office => office.id === this.selectedOfficeId);
  }

  onLocationChange(): void {
    this.loadOffices(this.selectedLocationId);
  }

  onOfficeChange(): void {
    console.log('selectedOfficeId:', this.selectedOfficeId);
  }

  selectDay(selectedDay: CalendarDay): void {
    this.calendarDays = this.calendarDays.map(day => ({
      ...day,
      isSelected: day.date.getTime() === selectedDay.date.getTime()
    }));
  }

  private generateCalendarDays(): void {
    const dayNames = ['V', 'H', 'K', 'Sze', 'Cs', 'P', 'Szo'];
    const today = new Date();

    this.calendarDays = Array.from({ length: 14 }, (_, index) => {
      const date = new Date(today);
      date.setDate(today.getDate() + index);

      return {
        date,
        dayLabel: dayNames[date.getDay()],
        dayNumber: date.getDate(),
        isSelected: index === 0
      };
    });
  }
}
