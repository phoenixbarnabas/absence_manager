import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import {
  catchError,
  debounceTime,
  finalize,
  forkJoin,
  map,
  Observable,
  of,
  Subject,
  switchMap,
  takeUntil,
  tap,
  timeout
} from 'rxjs';

import {
  CalendarAbsenceRequestType,
  CalendarDayInfoDto,
  CalendarDayView,
  CalendarEventDto,
  CalendarEventType,
  CalendarScope,
  CalendarViewMode,
  CreateAbsenceRequestDto
} from '../../models/calendar-models';
import { CalendarService } from '../../services/calendar-service';

@Component({
  selector: 'app-calendar-page',
  standalone: false,
  templateUrl: './calendar-page.html',
  styleUrl: './calendar-page.sass'
})
export class CalendarPage implements OnInit, OnDestroy {
  readonly weekDayNames = ['H', 'K', 'Sze', 'Cs', 'P', 'Szo', 'V'];

  readonly monthNames = [
    'Január',
    'Február',
    'Március',
    'Április',
    'Május',
    'Június',
    'Július',
    'Augusztus',
    'Szeptember',
    'Október',
    'November',
    'December'
  ];

  readonly viewModes: { value: CalendarViewMode; label: string; icon: string }[] = [
    { value: 'month', label: 'Havi', icon: 'bi-calendar3' },
    { value: 'week', label: 'Heti', icon: 'bi-calendar-week' }
  ];

  readonly scopeOptions: { value: CalendarScope; label: string; description: string }[] = [
    {
      value: 'mine',
      label: 'Saját',
      description: 'Csak a saját szabadság, home office és helyfoglalás eseményeim.'
    },
    {
      value: 'team',
      label: 'Csapat',
      description: 'A saját department / csapat eseményei.'
    },
    {
      value: 'organization',
      label: 'Szervezet',
      description: 'HR/Admin esetén szervezeti nézet, egyébként saját nézetre korlátozva.'
    }
  ];

  readonly eventTypeOptions: { value: CalendarEventType; label: string; icon: string }[] = [
    { value: 'vacation', label: 'Szabadság', icon: 'bi-suitcase-lg' },
    { value: 'homeOffice', label: 'Home office', icon: 'bi-house-door' },
    { value: 'sickLeave', label: 'Betegszabadság', icon: 'bi-heart-pulse' },
    { value: 'otherAbsence', label: 'Egyéb távollét', icon: 'bi-calendar-x' },
    { value: 'deskBooking', label: 'Helyfoglalás', icon: 'bi-grid-3x2-gap' }
  ];

  readonly absenceTypeOptions: { value: CalendarAbsenceRequestType; label: string; icon: string }[] = [
    { value: 'vacation', label: 'Szabadság', icon: 'bi-suitcase-lg' },
    { value: 'homeOffice', label: 'Home office', icon: 'bi-house-door' },
    { value: 'sickLeave', label: 'Betegszabadság', icon: 'bi-heart-pulse' },
    { value: 'otherAbsence', label: 'Egyéb távollét', icon: 'bi-calendar-x' }
  ];

  viewMode: CalendarViewMode = 'month';
  scope: CalendarScope = 'mine';

  currentDate = this.startOfDay(new Date());
  selectedDateKey = this.formatDateForApi(new Date());

  calendarDays: CalendarDayView[] = [];
  selectedEvent: CalendarEventDto | null = null;

  eventTypeSelection: Record<CalendarEventType, boolean> = {
    vacation: true,
    homeOffice: true,
    sickLeave: true,
    otherAbsence: true,
    deskBooking: true
  };

  loading = false;
  saving = false;

  errorMessage = '';
  warningMessage = '';
  successMessage = '';

  requestModalOpen = false;
  requestModalDay: CalendarDayView | null = null;

  requestForm: CreateAbsenceRequestDto = {
    type: 'vacation',
    dateFrom: this.selectedDateKey,
    dateTo: this.selectedDateKey,
    reason: ''
  };

  private dayInfos: CalendarDayInfoDto[] = [];
  private events: CalendarEventDto[] = [];

  private rangeFrom = this.currentDate;
  private rangeTo = this.currentDate;

  private readonly reloadRequested$ = new Subject<void>();
  private readonly destroy$ = new Subject<void>();

  private loadVersion = 0;
  private destroyed = false;

  constructor(
    private calendarService: CalendarService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.reloadRequested$
      .pipe(
        debounceTime(120),
        switchMap(() => this.fetchCalendar()),
        takeUntil(this.destroy$)
      )
      .subscribe();

    this.loadCalendar();
  }

  ngOnDestroy(): void {
    this.destroyed = true;
    this.destroy$.next();
    this.destroy$.complete();
  }

  get periodTitle(): string {
    if (this.viewMode === 'week') {
      const range = this.getCurrentRange();
      return `${this.formatShortDate(range.from)} - ${this.formatShortDate(range.to)}`;
    }

    return `${this.currentDate.getFullYear()}. ${this.monthNames[this.currentDate.getMonth()]}`;
  }

  get selectedDateTitle(): string {
    return this.formatLongDate(this.parseDateKey(this.selectedDateKey));
  }

  get selectedDayEvents(): CalendarEventDto[] {
    const selectedDay = this.calendarDays.find(day => day.dateKey === this.selectedDateKey);
    return selectedDay?.events ?? [];
  }

  get activeEventTypes(): CalendarEventType[] {
    return this.eventTypeOptions
      .map(option => option.value)
      .filter(type => this.eventTypeSelection[type]);
  }

  get eventCount(): number {
    return this.events.length;
  }

  get selectedScopeDescription(): string {
    return this.scopeOptions.find(option => option.value === this.scope)?.description ?? '';
  }

  get requestModalTitle(): string {
    if (!this.requestModalDay) {
      return 'Új igény';
    }

    return `${this.formatLongDate(this.requestModalDay.date)} - új igény`;
  }

  get canSubmitRequest(): boolean {
    if (!this.requestForm.type || !this.requestForm.dateFrom || !this.requestForm.dateTo) {
      return false;
    }

    if (this.requestForm.dateTo < this.requestForm.dateFrom) {
      return false;
    }

    if (this.requestForm.dateFrom < this.formatDateForApi(new Date())) {
      return false;
    }

    return !this.saving;
  }

  setViewMode(mode: CalendarViewMode): void {
    if (this.viewMode === mode) {
      return;
    }

    this.viewMode = mode;
    this.selectedEvent = null;
    this.loadCalendar();
  }

  previousPeriod(): void {
    if (this.viewMode === 'month') {
      this.currentDate = new Date(
        this.currentDate.getFullYear(),
        this.currentDate.getMonth() - 1,
        1
      );
    } else {
      const previousWeek = new Date(this.currentDate);
      previousWeek.setDate(previousWeek.getDate() - 7);
      this.currentDate = previousWeek;
    }

    this.selectedDateKey = this.formatDateForApi(this.currentDate);
    this.selectedEvent = null;
    this.loadCalendar();
  }

  nextPeriod(): void {
    if (this.viewMode === 'month') {
      this.currentDate = new Date(
        this.currentDate.getFullYear(),
        this.currentDate.getMonth() + 1,
        1
      );
    } else {
      const nextWeek = new Date(this.currentDate);
      nextWeek.setDate(nextWeek.getDate() + 7);
      this.currentDate = nextWeek;
    }

    this.selectedDateKey = this.formatDateForApi(this.currentDate);
    this.selectedEvent = null;
    this.loadCalendar();
  }

  goToToday(): void {
    this.currentDate = this.startOfDay(new Date());
    this.selectedDateKey = this.formatDateForApi(this.currentDate);
    this.selectedEvent = null;
    this.loadCalendar();
  }

  onScopeChange(scope: CalendarScope): void {
    this.scope = scope;
    this.selectedEvent = null;
    this.loadCalendar();
  }

  onEventTypeToggle(type: CalendarEventType, checked: boolean): void {
    this.eventTypeSelection[type] = checked;
    this.selectedEvent = null;
    this.loadCalendar();
  }

  selectDay(day: CalendarDayView): void {
    this.selectedDateKey = day.dateKey;
    this.selectedEvent = null;

    this.calendarDays = this.calendarDays.map(calendarDay => ({
      ...calendarDay,
      isSelected: calendarDay.dateKey === day.dateKey
    }));

    this.openRequestModal(day);
    this.refreshView();
  }

  openRequestModal(day: CalendarDayView): void {
    this.clearMessages();

    this.requestModalDay = day;
    this.requestModalOpen = true;

    this.requestForm = {
      type: 'vacation',
      dateFrom: day.dateKey,
      dateTo: day.dateKey,
      reason: ''
    };

    if (!day.isWorkingDay) {
      this.warningMessage = day.holidayName
        ? `A kiválasztott nap ünnepnap: ${day.holidayName}.`
        : 'A kiválasztott nap hétvége vagy nem munkanap.';
    }

    this.refreshView();
  }

  closeRequestModal(): void {
    if (this.saving) {
      return;
    }

    this.requestModalOpen = false;
    this.requestModalDay = null;
    this.refreshView();
  }

  submitRequest(): void {
    if (!this.canSubmitRequest) {
      this.errorMessage = 'Ellenőrizd a dátumokat és az igény típusát.';
      this.refreshView();
      return;
    }

    this.clearMessages();
    this.saving = true;
    this.refreshView();

    const dto: CreateAbsenceRequestDto = {
      type: this.requestForm.type,
      dateFrom: this.requestForm.dateFrom,
      dateTo: this.requestForm.dateTo,
      reason: this.requestForm.reason?.trim() || null
    };

    this.calendarService.createAbsenceRequest(dto)
      .pipe(
        timeout(15000),
        finalize(() => {
          this.saving = false;
          this.refreshView();
        })
      )
      .subscribe({
        next: () => {
          this.successMessage = 'Az igény mentése sikerült.';
          this.selectedDateKey = dto.dateFrom;
          this.requestModalOpen = false;
          this.requestModalDay = null;
          this.loadCalendar();
        },
        error: err => {
          console.error('Absence request save failed', err);
          this.errorMessage = this.getApiErrorMessage(err, 'Nem sikerült menteni az igényt.');
          this.refreshView();
        }
      });
  }

  selectEvent(event: CalendarEventDto, domEvent?: Event): void {
    domEvent?.stopPropagation();

    this.selectedEvent = event;
    this.selectedDateKey = event.dateFrom;

    this.calendarDays = this.calendarDays.map(calendarDay => ({
      ...calendarDay,
      isSelected: calendarDay.dateKey === event.dateFrom
    }));

    this.refreshView();
  }

  openSelectedEvent(): void {
    if (!this.selectedEvent?.detailsUrl) {
      return;
    }

    this.router.navigateByUrl(this.selectedEvent.detailsUrl);
  }

  reload(): void {
    this.loadCalendar();
  }

  trackByDate(_: number, day: CalendarDayView): string {
    return day.dateKey;
  }

  trackByEvent(_: number, event: CalendarEventDto): string {
    return event.id;
  }

  getEventTypeLabel(type: CalendarEventType): string {
    return this.eventTypeOptions.find(option => option.value === type)?.label ?? type;
  }

  getEventTypeIcon(type: CalendarEventType): string {
    return this.eventTypeOptions.find(option => option.value === type)?.icon ?? 'bi-calendar-event';
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'approved':
        return 'Jóváhagyva';
      case 'pending':
        return 'Függőben';
      case 'rejected':
        return 'Elutasítva';
      case 'cancelled':
        return 'Lemondva';
      default:
        return 'Információ';
    }
  }

  getEventDateText(event: CalendarEventDto): string {
    if (event.dateFrom === event.dateTo) {
      return this.formatLongDate(this.parseDateKey(event.dateFrom));
    }

    return `${this.formatLongDate(this.parseDateKey(event.dateFrom))} - ${this.formatLongDate(this.parseDateKey(event.dateTo))}`;
  }

  private loadCalendar(): void {
    this.reloadRequested$.next();
  }

  private fetchCalendar(): Observable<void> {
    const range = this.getCurrentRange();
    const currentLoadVersion = ++this.loadVersion;

    this.rangeFrom = range.from;
    this.rangeTo = range.to;

    const fromDate = this.formatDateForApi(range.from);
    const toDate = this.formatDateForApi(range.to);
    const selectedTypes = this.activeEventTypes;

    this.loading = true;
    this.errorMessage = '';
    this.warningMessage = '';

    this.dayInfos = this.generateFallbackDayInfos(range.from, range.to);
    this.events = [];
    this.rebuildView();
    this.refreshView();

    const dayInfos$ = this.calendarService.getDayInfos(fromDate, toDate).pipe(
      timeout(10000),
      catchError(err => {
        console.error('Calendar day-infos load failed', err);
        this.warningMessage = 'A munkanap/ünnepnap adatok nem töltődtek be időben, ezért ideiglenes helyi naptárlogikát használok.';
        return of(this.generateFallbackDayInfos(range.from, range.to));
      })
    );

    const events$ = selectedTypes.length === 0
      ? of([] as CalendarEventDto[])
      : this.calendarService.getEvents(fromDate, toDate, this.scope, selectedTypes).pipe(
        timeout(10000),
        catchError(err => {
          console.error('Calendar events load failed', err);
          this.errorMessage = this.getApiErrorMessage(err, 'Nem sikerült betölteni a naptár eseményeit.');
          return of([] as CalendarEventDto[]);
        })
      );

    return forkJoin({
      dayInfos: dayInfos$,
      events: events$
    }).pipe(
      tap(({ dayInfos, events }) => {
        if (currentLoadVersion !== this.loadVersion) {
          return;
        }

        this.dayInfos = dayInfos;
        this.events = events.map(event => this.normalizeEvent(event));
        this.rebuildView();
      }),
      catchError(err => {
        if (currentLoadVersion !== this.loadVersion) {
          return of(undefined);
        }

        console.error('Unexpected calendar load error', err);
        this.errorMessage = 'Váratlan hiba történt a naptár betöltése közben.';
        this.dayInfos = this.generateFallbackDayInfos(range.from, range.to);
        this.events = [];
        this.rebuildView();

        return of(undefined);
      }),
      finalize(() => {
        if (currentLoadVersion === this.loadVersion) {
          this.loading = false;
          this.refreshView();
        }
      }),
      map(() => undefined)
    );
  }

  private rebuildView(): void {
    const dayInfoMap = new Map(this.dayInfos.map(dayInfo => [dayInfo.date, dayInfo]));
    const eventsByDate = this.createEventsByDateMap(this.events);
    const days: CalendarDayView[] = [];

    for (let date = new Date(this.rangeFrom); date <= this.rangeTo; date.setDate(date.getDate() + 1)) {
      const currentDate = new Date(date);
      const dateKey = this.formatDateForApi(currentDate);
      const dayInfo = dayInfoMap.get(dateKey) ?? this.generateFallbackDayInfo(currentDate);

      days.push({
        date: currentDate,
        dateKey,
        dayNumber: currentDate.getDate(),
        dayName: this.weekDayNames[this.getMondayBasedWeekdayIndex(currentDate)],
        isToday: this.isSameDate(currentDate, new Date()),
        isCurrentMonth: this.viewMode !== 'month' || currentDate.getMonth() === this.currentDate.getMonth(),
        isSelected: dateKey === this.selectedDateKey,
        isWeekend: dayInfo.isWeekend,
        isHoliday: dayInfo.isHoliday,
        isWorkingDay: dayInfo.isWorkingDay,
        holidayName: dayInfo.holidayName,
        events: eventsByDate.get(dateKey) ?? []
      });
    }

    this.calendarDays = days;

    if (this.selectedEvent) {
      const refreshedEvent = this.events.find(event => event.id === this.selectedEvent?.id);
      this.selectedEvent = refreshedEvent ?? null;
    }
  }

  private createEventsByDateMap(events: CalendarEventDto[]): Map<string, CalendarEventDto[]> {
    const result = new Map<string, CalendarEventDto[]>();

    events.forEach(event => {
      const from = this.parseDateKey(event.dateFrom);
      const to = this.parseDateKey(event.dateTo);

      for (let date = new Date(from); date <= to; date.setDate(date.getDate() + 1)) {
        const dateKey = this.formatDateForApi(date);

        if (dateKey < this.formatDateForApi(this.rangeFrom) || dateKey > this.formatDateForApi(this.rangeTo)) {
          continue;
        }

        const existing = result.get(dateKey) ?? [];
        existing.push(event);
        result.set(dateKey, existing);
      }
    });

    return result;
  }

  private getCurrentRange(): { from: Date; to: Date } {
    if (this.viewMode === 'week') {
      const weekStart = this.getStartOfWeek(this.currentDate);
      const weekEnd = new Date(weekStart);
      weekEnd.setDate(weekStart.getDate() + 6);

      return {
        from: weekStart,
        to: weekEnd
      };
    }

    const firstDayOfMonth = new Date(
      this.currentDate.getFullYear(),
      this.currentDate.getMonth(),
      1
    );

    const lastDayOfMonth = new Date(
      this.currentDate.getFullYear(),
      this.currentDate.getMonth() + 1,
      0
    );

    return {
      from: this.getStartOfWeek(firstDayOfMonth),
      to: this.getEndOfWeek(lastDayOfMonth)
    };
  }

  private getStartOfWeek(date: Date): Date {
    const result = this.startOfDay(date);
    const dayOfWeek = result.getDay();
    const diff = dayOfWeek === 0 ? -6 : 1 - dayOfWeek;

    result.setDate(result.getDate() + diff);

    return result;
  }

  private getEndOfWeek(date: Date): Date {
    const result = this.getStartOfWeek(date);
    result.setDate(result.getDate() + 6);

    return result;
  }

  private generateFallbackDayInfos(from: Date, to: Date): CalendarDayInfoDto[] {
    const result: CalendarDayInfoDto[] = [];

    for (let date = new Date(from); date <= to; date.setDate(date.getDate() + 1)) {
      result.push(this.generateFallbackDayInfo(date));
    }

    return result;
  }

  private generateFallbackDayInfo(date: Date): CalendarDayInfoDto {
    const day = date.getDay();
    const isWeekend = day === 0 || day === 6;

    return {
      date: this.formatDateForApi(date),
      isWeekend,
      isHoliday: false,
      isWorkingDay: !isWeekend,
      holidayName: null
    };
  }

  private normalizeEvent(event: CalendarEventDto): CalendarEventDto {
    return {
      ...event,
      type: this.normalizeEventType(event.type),
      status: this.normalizeStatus(event.status),
      dateFrom: event.dateFrom.substring(0, 10),
      dateTo: event.dateTo.substring(0, 10)
    };
  }

  private normalizeEventType(type: string): CalendarEventType {
    switch (type) {
      case 'Vacation':
      case 'vacation':
        return 'vacation';
      case 'HomeOffice':
      case 'homeOffice':
        return 'homeOffice';
      case 'SickLeave':
      case 'sickLeave':
        return 'sickLeave';
      case 'OtherAbsence':
      case 'otherAbsence':
        return 'otherAbsence';
      case 'DeskBooking':
      case 'deskBooking':
        return 'deskBooking';
      default:
        return 'otherAbsence';
    }
  }

  private normalizeStatus(status: string): 'approved' | 'pending' | 'rejected' | 'cancelled' | 'info' {
    switch (status) {
      case 'Approved':
      case 'approved':
        return 'approved';
      case 'Pending':
      case 'pending':
        return 'pending';
      case 'Rejected':
      case 'rejected':
        return 'rejected';
      case 'Cancelled':
      case 'cancelled':
        return 'cancelled';
      default:
        return 'info';
    }
  }

  private clearMessages(): void {
    this.errorMessage = '';
    this.warningMessage = '';
    this.successMessage = '';
  }

  private getApiErrorMessage(err: any, fallback: string): string {
    if (err?.error?.message) {
      return err.error.message;
    }

    if (err?.status === 0) {
      return 'A backend nem érhető el. Ellenőrizd, hogy fut-e az API és jó-e az apiUrl.';
    }

    if (err?.status === 401) {
      return 'A munkamenet lejárt vagy nincs jogosultságod. Jelentkezz be újra.';
    }

    if (err?.status === 403) {
      return 'Ehhez a művelethez nincs megfelelő jogosultságod.';
    }

    if (err?.name === 'TimeoutError') {
      return 'A backend nem válaszolt időben. Ellenőrizd, hogy fut-e az API.';
    }

    return fallback;
  }

  private refreshView(): void {
    this.cdr.markForCheck();

    queueMicrotask(() => {
      if (this.destroyed) {
        return;
      }

      try {
        this.cdr.detectChanges();
      } catch {
        // A komponens már lehet, hogy közben megsemmisült route váltás miatt.
      }
    });
  }

  private formatDateForApi(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  private parseDateKey(dateKey: string): Date {
    const [year, month, day] = dateKey.substring(0, 10).split('-').map(Number);
    return new Date(year, month - 1, day);
  }

  private formatShortDate(date: Date): string {
    return `${this.monthNames[date.getMonth()].slice(0, 3)} ${date.getDate()}.`;
  }

  private formatLongDate(date: Date): string {
    return `${date.getFullYear()}. ${this.monthNames[date.getMonth()]} ${date.getDate()}.`;
  }

  private startOfDay(date: Date): Date {
    return new Date(
      date.getFullYear(),
      date.getMonth(),
      date.getDate()
    );
  }

  private isSameDate(first: Date, second: Date): boolean {
    return first.getFullYear() === second.getFullYear()
      && first.getMonth() === second.getMonth()
      && first.getDate() === second.getDate();
  }

  private getMondayBasedWeekdayIndex(date: Date): number {
    const day = date.getDay();
    return day === 0 ? 6 : day - 1;
  }
}