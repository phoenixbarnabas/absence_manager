import { Component, OnInit } from '@angular/core';
import { BehaviorSubject, catchError, combineLatest, distinctUntilChanged, Observable, of, shareReplay, switchMap, tap } from 'rxjs';
import { Location, Office, Workstation } from '../../models/entity-models';
import { LocationService } from '../../services/location-service';
import { OfficeService } from '../../services/office-service';
import { BookingService } from '../../services/booking-service';
import { OfficeDayAvailabilityDto } from '../../models/availability-models';
import { HttpClient } from '@angular/common/http';

type CalendarDay = {
  date: Date;
  dayLabel: string;
  dayNumber: number;
  isSelected: boolean;
  isWeekend: boolean;
  isHoliday: boolean;
  holidayName?: string;
};

type PublicHoliday = {
  date: string;
  localName: string;
  name: string;
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

  selectedLocationId = '';
  selectedOfficeId = '';
  selectedWorkstationId = '';
  selectedDate: Date = new Date();

  isSubmittingBooking = false;
  errorMessage = '';
  successMessage = '';
  currentBookingId: string | null = null;

  private availabilityRefreshSubject = new BehaviorSubject<number>(0);
  readonly availabilityRefresh$ = this.availabilityRefreshSubject.asObservable();

  private selectedLocationIdSubject = new BehaviorSubject<string>('');
  private selectedOfficeIdSubject = new BehaviorSubject<string>('');
  private selectedDateSubject = new BehaviorSubject<string>(this.formatDateForApi(new Date()));

  readonly selectedLocationId$ = this.selectedLocationIdSubject.asObservable();
  readonly selectedOfficeId$ = this.selectedOfficeIdSubject.asObservable();
  readonly selectedDate$ = this.selectedDateSubject.asObservable();

  locations$!: Observable<Location[]>;
  offices$!: Observable<Office[]>;
  availability$!: Observable<OfficeDayAvailabilityDto | null>;

  constructor(
    private locationService: LocationService,
    private officeService: OfficeService,
    private bookingService: BookingService,
    private http: HttpClient,
  ) { }

  ngOnInit(): void {
    this.generateCalendarDays();
    this.loadHolidaysForVisibleYears();
    this.restoreState();

    this.locations$ = this.locationService.loadAll().pipe(
      catchError(err => {
        console.error(err);
        this.errorMessage = 'Nem sikerült betölteni a telephelyeket.';
        return of([]);
      }),
      shareReplay(1)
    );

    this.offices$ = this.selectedLocationId$.pipe(
      distinctUntilChanged(),
      switchMap(locationId => {
        if (!locationId) {
          return of([]);
        }

        return this.officeService.loadAllByLocationId(locationId).pipe(
          catchError(err => {
            console.error(err);
            this.errorMessage = 'Nem sikerült betölteni az irodákat.';
            return of([]);
          })
        );
      }),
      tap(offices => {
        if (!this.selectedOfficeId) {
          return;
        }

        const exists = offices.some(office => office.id === this.selectedOfficeId);

        if (!exists) {
          this.selectedOfficeId = '';
          this.selectedWorkstationId = '';
          this.currentBookingId = null;
          this.selectedOfficeIdSubject.next('');
          this.saveState();
        }
      }),
      shareReplay(1)
    );

    this.availability$ = combineLatest([
      this.selectedOfficeId$,
      this.selectedDate$,
      this.availabilityRefresh$
    ]).pipe(
      switchMap(([officeId, date]) => {
        if (!officeId || !date) {
          this.currentBookingId = null;
          this.selectedWorkstationId = '';
          return of(null);
        }

        this.errorMessage = '';
        this.successMessage = '';

        return this.bookingService.getAvailability(officeId, date).pipe(
          tap(availability => {
            if (
              this.selectedWorkstationId &&
              !availability.workstations.some(ws => ws.workstationId === this.selectedWorkstationId)
            ) {
              this.selectedWorkstationId = '';
            }

            if (availability.currentUserHasBooking && availability.currentUserWorkstationId) {
              this.selectedWorkstationId = availability.currentUserWorkstationId;
              this.currentBookingId = availability.currentUserBookingId ?? null;
            } else {
              this.currentBookingId = null;
            }

            this.saveState();
          }),
          catchError(err => {
            console.error(err);
            this.currentBookingId = null;
            this.errorMessage = 'Nem sikerült betölteni az elérhetőségi adatokat.';
            return of(null);
          })
        );
      }),
      shareReplay(1)
    );

    this.selectedLocationIdSubject.next(this.selectedLocationId);
    this.selectedOfficeIdSubject.next(this.selectedOfficeId);
    this.selectedDateSubject.next(this.selectedDateString);
  }

  onLocationChange(locationId: string): void {
    this.selectedLocationId = locationId;
    this.selectedOfficeId = '';
    this.selectedWorkstationId = '';
    this.currentBookingId = null;
    this.errorMessage = '';
    this.successMessage = '';

    this.selectedLocationIdSubject.next(locationId);
    this.selectedOfficeIdSubject.next('');
    this.saveState();
  }

  onOfficeChange(officeId: string): void {
    this.selectedOfficeId = officeId;
    this.selectedWorkstationId = '';
    this.currentBookingId = null;
    this.errorMessage = '';
    this.successMessage = '';

    this.selectedOfficeIdSubject.next(officeId);
    this.saveState();
  }

  onWorkstationSelected(workstationId: string, availability: OfficeDayAvailabilityDto | null): void {
    if (availability?.currentUserHasBooking && this.selectedWorkstationId !== workstationId) {
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
    this.currentBookingId = null;
    this.selectedDateSubject.next(this.selectedDateString);
    this.saveState();
  }

  submitBooking(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.selectedOfficeId) {
      this.errorMessage = 'Előbb válassz irodát.';
      return;
    }

    if (!this.selectedWorkstationId) {
      this.errorMessage = 'Válassz egy munkaállomást.';
      return;
    }

    this.isSubmittingBooking = true;

    this.bookingService.createBooking(
      this.selectedWorkstationId,
      this.selectedDateString
    ).subscribe({
      next: () => {
        this.successMessage = 'A foglalás sikeresen létrejött.';
        this.isSubmittingBooking = false;

        this.availabilityRefreshSubject.next(Date.now());
        this.selectedOfficeIdSubject.next(this.selectedOfficeId);
      },
      error: err => {
        console.error(err);
        this.errorMessage =
          err?.error?.message ?? 'Nem sikerült létrehozni a foglalást.';
        this.isSubmittingBooking = false;
      }
    });
  }

  cancelBooking(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.currentBookingId) {
      this.errorMessage = 'Nincs lemondható foglalás.';
      return;
    }

    this.isSubmittingBooking = true;

    this.bookingService.cancelBooking(this.currentBookingId).subscribe({
      next: () => {
        this.successMessage = 'A foglalás sikeresen lemondva.';
        this.isSubmittingBooking = false;
        this.currentBookingId = null;
        this.selectedWorkstationId = '';
        this.availabilityRefreshSubject.next(Date.now());

        this.selectedOfficeIdSubject.next(this.selectedOfficeId);
      },
      error: err => {
        console.error(err);
        this.errorMessage =
          err?.error?.message ?? 'Nem sikerült lemondani a foglalást.';
        this.isSubmittingBooking = false;
      }
    });
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
        isSelected: index === 0,
        isWeekend: date.getDay() === 0 || date.getDay() === 6,
        isHoliday: false
      };
    });

    this.selectedDate = this.calendarDays[0].date;
  }

  goToPreviousDay(): void {
    this.changeSelectedDateBy(-1);
  }

  goToNextDay(): void {
    this.changeSelectedDateBy(1);
  }

  private normalizeDate(date: Date): Date {
    const normalized = new Date(date);
    normalized.setHours(0, 0, 0, 0);
    return normalized;
  }

  private changeSelectedDateBy(dayOffset: number): void {
    const newDate = this.normalizeDate(this.selectedDate);
    newDate.setDate(newDate.getDate() + dayOffset);

    const today = this.getToday();
    const maxDate = this.getMaxBookableDate();

    if (newDate < today || newDate > maxDate) {
      return;
    }

    this.selectedDate = newDate;
    this.selectedWorkstationId = '';
    this.currentBookingId = null;

    this.calendarDays = this.calendarDays.map(day => ({
      ...day,
      isSelected: this.isSameDate(day.date, newDate)
    }));

    this.selectedDateSubject.next(this.selectedDateString);
    this.saveState();
  }

  private generateCalendarDaysFrom(startDate: Date): void {
    const dayNames = ['V', 'H', 'K', 'Sze', 'Cs', 'P', 'Szo'];

    this.calendarDays = Array.from({ length: 14 }, (_, index) => {
      const date = new Date(startDate);
      date.setDate(startDate.getDate() + index);

      return {
        date,
        dayLabel: dayNames[date.getDay()],
        dayNumber: date.getDate(),
        isSelected: index === 0,
        isWeekend: date.getDay() === 0 || date.getDay() === 6,
        isHoliday: false
      };
    });
  }

  private loadHolidaysForVisibleYears(): void {
    const years = [...new Set(this.calendarDays.map(day => day.date.getFullYear()))];

    years.forEach(year => {
      this.http.get<PublicHoliday[]>(`https://date.nager.at/api/v3/PublicHolidays/${year}/HU`)
        .subscribe({
          next: holidays => {
            this.calendarDays = this.calendarDays.map(day => {
              const dateString = this.formatDateForApi(day.date);
              const holiday = holidays.find(h => h.date === dateString);

              return {
                ...day,
                isHoliday: !!holiday,
                holidayName: holiday?.localName
              };
            });
          },
          error: err => {
            console.error('Ünnepnapok betöltése sikertelen', err);
          }
        });
    });
  }

  isSelectedDateNonWorkingDay(): boolean {
    const selectedDay = this.calendarDays.find(day =>
      this.isSameDate(day.date, this.selectedDate)
    );

    return !!selectedDay?.isWeekend || !!selectedDay?.isHoliday;
  }

  getSelectedDateLabel(): string {
    const selectedDay = this.calendarDays.find(day =>
      this.isSameDate(day.date, this.selectedDate)
    );

    if (selectedDay?.isHoliday) {
      return `Ünnepnap: ${selectedDay.holidayName}`;
    }

    if (selectedDay?.isWeekend) {
      return 'Hétvége';
    }

    return 'Munkanap';
  }

  readonly maxBookableDays = 14;

  get isFirstBookableDay(): boolean {
    return this.isSameDate(this.selectedDate, this.getToday());
  }

  get isLastBookableDay(): boolean {
    return this.isSameDate(this.selectedDate, this.getMaxBookableDate());
  }

  private getToday(): Date {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return today;
  }

  private getMaxBookableDate(): Date {
    const maxDate = this.getToday();
    maxDate.setDate(maxDate.getDate() + this.maxBookableDays - 1);
    return maxDate;
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

  private isSameDate(first: Date, second: Date): boolean {
    return first.getFullYear() === second.getFullYear()
      && first.getMonth() === second.getMonth()
      && first.getDate() === second.getDate();
  }
}