import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LocationService } from '../../services/location-service';
import { Observable, tap } from 'rxjs';
import { Location, Office, Workstation } from '../../models/entity-models';
import { OfficeService } from '../../services/office-service';
import { WorkstationService } from '../../services/workstation-service';
import { BookingService } from '../../services/booking-service';
import { OfficeDayAvailabilityDto } from '../../models/availability-models';

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
  workstations$!: Observable<Workstation[]>;

  selectedLocationId!: string
  selectedOfficeId!: string
  selectedWorkstationId!: string

  availability: OfficeDayAvailabilityDto | null = null;
  selectedDate: Date = new Date();

  isLoadingAvailability = false;
  isLoadingMyBookings = false;
  isSubmittingBooking = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private locationService: LocationService,
    private officeService: OfficeService,
    private workstationService: WorkstationService,
    private bookingService: BookingService
  ) { }

  ngOnInit(): void {
    this.generateCalendarDays()
    this.loadLocations()
    this.workstations$ = this.workstationService.workstations$;
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
    this.selectedWorkstationId = '';
    this.workstationService.clear();

    this.officeService.loadAllByLocationId(locationId).subscribe();

    this.offices$ = this.officeService.offices$.pipe(
      tap(offices => {
        if (!this.selectedOfficeId && offices.length > 0) {
          this.selectedOfficeId = offices[0].id;
          this.loadWorkstations(this.selectedOfficeId);
          this.loadAvailability();
        }
      })
    );
  }

  loadWorkstations(officeId: string): void {
    if (!officeId) {
      return;
    }

    this.selectedWorkstationId = '';
    this.workstationService.loadAllByOfficeId(officeId).subscribe();
  }

  loadAvailability(): void {
    if (!this.selectedOfficeId || !this.selectedDateString) {
      this.availability = null;
      this.workstationService.clear();
      return;
    }

    this.isLoadingAvailability = true;
    this.errorMessage = '';

    this.bookingService.getAvailability(this.selectedOfficeId, this.selectedDateString)
      .subscribe({
        next: availability => {
          this.availability = availability;
          this.selectedWorkstationId = availability.currentUserWorkstationId ?? '';
          this.isLoadingAvailability = false;
        },
        error: err => {
          console.error(err);
          this.availability = null;
          this.isLoadingAvailability = false;
          this.errorMessage = 'Nem sikerült betölteni az elérhetőségi adatokat.';
        }
      });
  }

  currentLocation$(locations: Location[]): Location | undefined {
    return locations.find(location => location.id === this.selectedLocationId);
  }

  currentOffice$(offices: Office[]): Office | undefined {
    return offices.find(office => office.id === this.selectedOfficeId);
  }

  onLocationChange(locationId: string): void {
    this.selectedLocationId = locationId;
    this.selectedOfficeId = '';
    this.selectedWorkstationId = '';
    this.availability = null;
    this.loadOffices(locationId);
  }

  onOfficeChange(officeId: string): void {
    this.selectedOfficeId = officeId;
    this.selectedWorkstationId = '';
    this.loadAvailability();
  }

  onWorkstationSelected(workstationId: string): void {
    this.selectedWorkstationId = workstationId;
  }

  selectDay(selectedDay: CalendarDay): void {
    this.selectedDate = selectedDay.date;

    this.calendarDays = this.calendarDays.map(day => ({
      ...day,
      isSelected: day.date.getTime() === selectedDay.date.getTime()
    }));

    this.selectedWorkstationId = '';
    this.loadAvailability();
  }

  get selectedDateString(): string {
    const date = this.selectedDate;
    if (!date) {
      return '';
    }

    return this.formatDateForApi(date);
  }

  private formatDateForApi(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
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
