import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { Location, Office, Workstation } from '../../models/entity-models';
import { LocationService } from '../../services/location-service';
import { OfficeService } from '../../services/office-service';
import { WorkstationService } from '../../services/workstation-service';
import { BookingService } from '../../services/booking-service';
import { OfficeDayAvailabilityDto } from '../../models/availability-models';

type CalendarDay = {
  date: Date;
  dayLabel: string;
  dayNumber: number;
  isSelected: boolean;
};

type DeskBookingState = {
  selectedLocationId: string;
  selectedOfficeId: string;
  selectedWorkstationId: string;
  selectedDate: string;
};

@Component({
  selector: 'app-desk-booking',
  standalone: false,
  templateUrl: './desk-booking.html',
  styleUrl: './desk-booking.sass',
})
export class DeskBooking implements OnInit {
  private readonly storageKey = 'desk-booking-state';

  calendarDays: CalendarDay[] = [];

  locations$!: Observable<Location[]>;
  offices$!: Observable<Office[]>;
  workstations$!: Observable<Workstation[]>;

  selectedLocationId = '';
  selectedOfficeId = '';
  selectedWorkstationId = '';

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
    private bookingService: BookingService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.generateCalendarDays();
    this.restoreState();
    this.bindStreams();
    this.loadLocations();
  }

  private bindStreams(): void {
    this.locations$ = this.locationService.location$;
    this.offices$ = this.officeService.offices$;
    this.workstations$ = this.workstationService.workstations$;
  }

  loadLocations(): void {
    this.locationService.loadAll().subscribe({
      next: locations => {
        const hasSavedLocation =
          !!this.selectedLocationId &&
          locations.some(location => location.id === this.selectedLocationId);

        if (!hasSavedLocation) {
          this.resetLocationSelection();
          this.saveState();
          return;
        }

        this.loadOffices(this.selectedLocationId, true);
      },
      error: err => {
        console.error(err);
        this.errorMessage = 'Nem sikerült betölteni a telephelyeket.';
      }
    });
  }

  loadOffices(locationId: string, restoringState = false): void {
    if (!locationId) {
      this.resetLocationSelection();
      this.saveState();
      return;
    }

    if (!restoringState) {
      this.selectedOfficeId = '';
      this.selectedWorkstationId = '';
      this.availability = null;
      this.workstationService.clear();
    }

    this.officeService.loadAllByLocationId(locationId).subscribe({
      next: offices => {
        const hasSavedOffice =
          !!this.selectedOfficeId &&
          offices.some(office => office.id === this.selectedOfficeId);

        if (!hasSavedOffice) {
          this.selectedOfficeId = '';
          this.selectedWorkstationId = '';
          this.availability = null;
          this.workstationService.clear();
          this.saveState();
          return;
        }

        this.loadAvailability();
      },
      error: err => {
        console.error(err);
        this.errorMessage = 'Nem sikerült betölteni az irodákat.';
      }
    });
  }

  loadAvailability(): void {
    if (!this.selectedOfficeId || !this.selectedDateString) {
      this.availability = null;
      this.workstationService.clear();
      this.saveState();
      return;
    }

    this.isLoadingAvailability = true;
    this.errorMessage = '';

    this.bookingService.getAvailability(this.selectedOfficeId, this.selectedDateString)
      .subscribe({
        next: availability => {
          this.availability = availability;

          if (
            this.selectedWorkstationId &&
            !availability.workstations.some(ws => ws.workstationId === this.selectedWorkstationId)
          ) {
            this.selectedWorkstationId = '';
          }

          if (
            availability.currentUserHasBooking &&
            availability.currentUserWorkstationId
          ) {
            this.selectedWorkstationId = availability.currentUserWorkstationId;
          }

          this.isLoadingAvailability = false;
          this.saveState();
          this.cdr.detectChanges();
        },
        error: err => {
          console.error(err);
          this.availability = null;
          this.isLoadingAvailability = false;
          this.errorMessage = 'Nem sikerült betölteni az elérhetőségi adatokat.';
          this.cdr.detectChanges();
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
    this.workstationService.clear();
    this.saveState();

    if (!locationId) {
      return;
    }

    this.loadOffices(locationId);
  }

  onOfficeChange(officeId: string): void {
    this.selectedOfficeId = officeId;
    this.selectedWorkstationId = '';
    this.availability = null;
    this.workstationService.clear();
    this.saveState();

    if (!officeId) {
      return;
    }

    this.loadAvailability();
  }

  onWorkstationSelected(workstationId: string): void {
    if (this.availability?.currentUserHasBooking && this.selectedWorkstationId !== workstationId) {
      return;
    }

    this.selectedWorkstationId =
      this.selectedWorkstationId === workstationId ? '' : workstationId;

    this.saveState();
  }

  selectDay(selectedDay: CalendarDay): void {
    this.selectedDate = selectedDay.date;

    this.calendarDays = this.calendarDays.map(day => ({
      ...day,
      isSelected: day.date.getTime() === selectedDay.date.getTime()
    }));

    this.selectedWorkstationId = '';
    this.saveState();

    if (this.selectedOfficeId) {
      this.loadAvailability();
    }
  }

  get selectedDateString(): string {
    return this.formatDateForApi(this.selectedDate);
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

    this.selectedDate = this.calendarDays[0].date;
  }

  private saveState(): void {
    const state: DeskBookingState = {
      selectedLocationId: this.selectedLocationId,
      selectedOfficeId: this.selectedOfficeId,
      selectedWorkstationId: this.selectedWorkstationId,
      selectedDate: this.selectedDateString
    };

    localStorage.setItem(this.storageKey, JSON.stringify(state));
  }

  private restoreState(): void {
    const rawState = localStorage.getItem(this.storageKey);

    if (!rawState) {
      return;
    }

    try {
      const state: DeskBookingState = JSON.parse(rawState);

      this.selectedLocationId = state.selectedLocationId ?? '';
      this.selectedOfficeId = state.selectedOfficeId ?? '';
      this.selectedWorkstationId = state.selectedWorkstationId ?? '';

      if (state.selectedDate) {
        const restoredDate = new Date(state.selectedDate);

        const existsInCalendar = this.calendarDays.some(day =>
          this.isSameDate(day.date, restoredDate)
        );

        if (existsInCalendar) {
          this.selectedDate = restoredDate;

          this.calendarDays = this.calendarDays.map(day => ({
            ...day,
            isSelected: this.isSameDate(day.date, restoredDate)
          }));
        }
      }
    } catch (error) {
      console.error('Nem sikerült visszaállítani a booking state-et.', error);
      localStorage.removeItem(this.storageKey);
    }
  }

  private resetLocationSelection(): void {
    this.selectedLocationId = '';
    this.selectedOfficeId = '';
    this.selectedWorkstationId = '';
    this.availability = null;
    this.workstationService.clear();
  }

  private isSameDate(first: Date, second: Date): boolean {
    return first.getFullYear() === second.getFullYear()
      && first.getMonth() === second.getMonth()
      && first.getDate() === second.getDate();
  }
}