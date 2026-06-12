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
import { NotificationService } from '../../services/notification-service';

type CalendarDisplayViewMode = CalendarViewMode | 'year';

interface CalendarYearMonthView {
  monthIndex: number;
  monthName: string;
  monthDate: Date;
  leadingBlankDays: number[];
  days: CalendarDayView[];
  eventCount: number;
  holidayCount: number;
}

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

  readonly viewModes: { value: CalendarDisplayViewMode; label: string; icon: string }[] = [
    { value: 'week', label: 'Heti', icon: 'bi-calendar-week' },
    { value: 'month', label: 'Havi', icon: 'bi-calendar3' },
    { value: 'year', label: 'Éves', icon: 'bi-calendar4-range' }
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
  ];

  readonly absenceTypeOptions: { value: CalendarAbsenceRequestType; label: string; icon: string }[] = [
    { value: 'vacation', label: 'Szabadság', icon: 'bi-suitcase-lg' },
    { value: 'homeOffice', label: 'Home office', icon: 'bi-house-door' },
    { value: 'sickLeave', label: 'Betegszabadság', icon: 'bi-heart-pulse' },
    { value: 'otherAbsence', label: 'Egyéb távollét', icon: 'bi-calendar-x' }
  ];

  viewMode: CalendarDisplayViewMode = 'month';
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
  cancellingEventId: string | null = null;
  cancelModalEvent: CalendarEventDto | null = null;

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
    private cdr: ChangeDetectorRef,
    private notificationService: NotificationService,
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
    if (this.viewMode === 'year') {
      return `${this.currentDate.getFullYear()}. év`;
    }

    if (this.viewMode === 'week') {
      const range = this.getCurrentRange();
      return `${this.formatShortDate(range.from)} - ${this.formatShortDate(range.to)}`;
    }

    return `${this.currentDate.getFullYear()}. ${this.monthNames[this.currentDate.getMonth()]}`;
  }

  get selectedDateTitle(): string {
    return this.formatLongDate(this.parseDateKey(this.selectedDateKey));
  }

  get selectedCalendarDay(): CalendarDayView | null {
    return this.calendarDays.find(day => day.dateKey === this.selectedDateKey) ?? null;
  }

  get selectedDayEvents(): CalendarEventDto[] {
    return this.selectedCalendarDay?.events ?? [];
  }

  get yearMonths(): CalendarYearMonthView[] {
    const year = this.currentDate.getFullYear();

    return this.monthNames.map((monthName, monthIndex) => {
      const days = this.calendarDays.filter(day =>
        day.date.getFullYear() === year && day.date.getMonth() === monthIndex
      );

      return {
        monthIndex,
        monthName,
        monthDate: new Date(year, monthIndex, 1),
        leadingBlankDays: Array.from({ length: this.getMondayBasedWeekdayIndex(new Date(year, monthIndex, 1)) }),
        days,
        eventCount: days.reduce((total, day) => total + day.events.length, 0),
        holidayCount: days.filter(day => day.isHoliday).length
      };
    });
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

  setViewMode(mode: CalendarDisplayViewMode): void {
    if (this.viewMode === mode) {
      return;
    }

    this.viewMode = mode;
    this.selectedEvent = null;
    this.loadCalendar();
  }

  previousPeriod(): void {
    if (this.viewMode === 'year') {
      this.currentDate = new Date(this.currentDate.getFullYear() - 1, 0, 1);
    } else if (this.viewMode === 'month') {
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
    if (this.viewMode === 'year') {
      this.currentDate = new Date(this.currentDate.getFullYear() + 1, 0, 1);
    } else if (this.viewMode === 'month') {
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

  openMonth(month: CalendarYearMonthView): void {
    this.currentDate = new Date(this.currentDate.getFullYear(), month.monthIndex, 1);
    this.selectedDateKey = this.formatDateForApi(this.currentDate);
    this.viewMode = 'month';
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

    if (this.activeEventTypes.length === 0) {
      this.notificationService.info('Nincs kiválasztva eseménytípus, ezért a naptár események nélkül jelenik meg.');
    }

    this.loadCalendar();
  }

  selectDay(day: CalendarDayView): void {
    this.clearMessages();
    this.selectedDateKey = day.dateKey;
    this.selectedEvent = null;

    this.calendarDays = this.calendarDays.map(calendarDay => ({
      ...calendarDay,
      isSelected: calendarDay.dateKey === day.dateKey
    }));

    this.refreshView();
  }

  openRequestModal(day: CalendarDayView): void {
    this.clearMessages();

    if (day.dateKey < this.formatDateForApi(new Date())) {
      this.selectedDateKey = day.dateKey;
      this.selectedEvent = null;

      this.calendarDays = this.calendarDays.map(calendarDay => ({
        ...calendarDay,
        isSelected: calendarDay.dateKey === day.dateKey
      }));

      this.notificationService.warning('Múltbeli napra nem lehet új igényt rögzíteni.');
      this.refreshView();
      return;
    }

    if (this.hasActiveAbsenceRequestOnDay(day)) {
      this.selectedDateKey = day.dateKey;
      this.selectedEvent = null;

      this.calendarDays = this.calendarDays.map(calendarDay => ({
        ...calendarDay,
        isSelected: calendarDay.dateKey === day.dateKey
      }));

      this.notificationService.warning('Erre a napra már van kérelmed.');
      this.refreshView();
      return;
    }

    this.requestModalDay = day;
    this.requestModalOpen = true;

    this.requestForm = {
      type: 'vacation',
      dateFrom: day.dateKey,
      dateTo: day.dateKey,
      reason: ''
    };

    if (!day.isWorkingDay) {
      this.notificationService.warning(
        day.holidayName
          ? `A kiválasztott nap ünnepnap: ${day.holidayName}.`
          : 'A kiválasztott nap hétvége vagy nem munkanap.'
      );
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
    if (this.saving) {
      return;
    }

    const validationMessage = this.getRequestValidationMessage();

    if (validationMessage) {
      this.notificationService.warning(validationMessage);
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
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => {
          this.notificationService.success('Az igény mentése sikerült.');
          this.selectedDateKey = dto.dateFrom;
          this.requestModalOpen = false;
          this.requestModalDay = null;
          this.loadCalendar();
        },
        error: err => {
          console.error('Absence request save failed', err);

          this.notificationService.error(
            this.notificationService.getMessage(err, 'Nem sikerült menteni az igényt.')
          );

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

  getEventDetailsUrl(event: CalendarEventDto | null): string {
    if (!event) {
      return '';
    }

    const apiDetailsUrl = event.detailsUrl?.trim();

    if (apiDetailsUrl) {
      return apiDetailsUrl;
    }

    if (event.type === 'deskBooking') {
      return '/desk-booking';
    }

    return `/my-absence-requests?requestId=${encodeURIComponent(event.id)}`;
  }

  canOpenRelatedPage(event: CalendarEventDto | null | undefined): boolean {
    if (!event) {
      return false;
    }

    if (event.type === 'deskBooking') {
      return true;
    }

    return !!this.getRelatedRequestId(event);
  }

  private hasActiveAbsenceRequestOnDay(day: CalendarDayView): boolean {
    return this.hasActiveAbsenceRequestInRange(day.dateKey, day.dateKey);
  }

  openSelectedEvent(event?: CalendarEventDto | null): void {
    const targetEvent = event ?? this.selectedEvent;

    if (!targetEvent) {
      return;
    }

    if (targetEvent.type === 'deskBooking') {
      this.router.navigate(['/desk-booking'], {
        queryParams: {
          date: targetEvent.dateFrom
        }
      });

      return;
    }

    const requestId = this.getRelatedRequestId(targetEvent);

    if (!requestId) {
      this.notificationService.warning('Ehhez az eseményhez nem található kapcsolódó kérelem azonosító.');
      this.refreshView();
      return;
    }

    this.router.navigate(['/my-absence-requests'], {
      queryParams: {
        requestId
      }
    });
  }

  canCancelCalendarEvent(event: CalendarEventDto | null | undefined): boolean {
    if (!event || this.saving || this.cancellingEventId) {
      return false;
    }

    if (event.type === 'deskBooking') {
      return false;
    }

    const status = this.normalizeStatus(event.status);

    if (status !== 'pending' && status !== 'approved') {
      return false;
    }

    return event.dateFrom >= this.formatDateForApi(new Date());
  }

  cancelSelectedEvent(): void {
    const event = this.selectedEvent;

    if (!this.canCancelCalendarEvent(event)) {
      return;
    }

    this.cancelModalEvent = event;
    this.refreshView();
  }

  closeCancelCalendarModal(): void {
    if (this.cancellingEventId) {
      return;
    }

    this.cancelModalEvent = null;
    this.refreshView();
  }

  confirmCancelSelectedEvent(): void {
    const event = this.cancelModalEvent;

    if (!this.canCancelCalendarEvent(event)) {
      return;
    }

    const requestId = this.getRelatedRequestId(event!);

    if (!requestId) {
      this.cancelModalEvent = null;
      this.notificationService.warning('Ehhez az eseményhez nem található visszavonható kérelem azonosító.');
      this.refreshView();
      return;
    }

    this.clearMessages();
    this.cancellingEventId = event!.id;
    this.refreshView();

    this.calendarService.cancelAbsenceRequest(requestId)
      .pipe(
        timeout(15000),
        finalize(() => {
          this.cancellingEventId = null;
          this.refreshView();
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => {
          this.notificationService.success('A kérelem visszavonása sikerült.');
          this.selectedEvent = null;
          this.cancelModalEvent = null;
          this.loadCalendar();
        },
        error: err => {
          console.error('Calendar absence request cancel failed', err);

          this.notificationService.error(
            this.notificationService.getMessage(
              err,
              'Nem sikerült visszavonni a kérelmet.'
            )
          );

          this.refreshView();
        }
      });
  }

  private getRelatedRequestId(event: CalendarEventDto): string | null {
    const extendedEvent = event as CalendarEventDto & {
      requestId?: string | null;
      absenceRequestId?: string | null;
      relatedEntityId?: string | null;
      entityId?: string | null;
      sourceId?: string | null;
    };

    return (
      extendedEvent.requestId ||
      extendedEvent.absenceRequestId ||
      extendedEvent.relatedEntityId ||
      extendedEvent.entityId ||
      extendedEvent.sourceId ||
      event.id ||
      null
    );
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

  canCreateRequestForDay(day: CalendarDayView): boolean {
    return !this.saving && day.dateKey >= this.formatDateForApi(new Date());
  }

  getSelectedDayStatusTitle(day: CalendarDayView): string {
    if (day.isHoliday) {
      return day.holidayName || 'Ünnepnap';
    }

    if (day.isWeekend) {
      return 'Hétvégi nap';
    }

    return 'Munkanap';
  }

  getSelectedDayStatusDescription(day: CalendarDayView): string {
    if (day.isHoliday) {
      return 'Az ünnepnap adat az ünnepnap API-ból érkezik, ezért az igénylés előtt külön jelölve jelenik meg.';
    }

    if (day.isWeekend) {
      return 'Nem munkanapként jelölt dátum. Igénylés előtt ellenőrizd, hogy valóban erre a napra szeretnél-e rögzíteni.';
    }

    if (day.events.length) {
      return `${day.events.length} esemény látható ezen a napon.`;
    }

    return 'Nincs látható esemény, innen indítható új igény erre a napra.';
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
      timeout(30000),
      catchError(err => {
        console.error('Calendar day-infos load failed', err);
        this.notificationService.warning(
          'A munkanap/ünnepnap adatok nem töltődtek be időben, ezért ideiglenes helyi naptárlogikát használok.'
        );
        return of(this.generateFallbackDayInfos(range.from, range.to));
      })
    );

    const events$ = selectedTypes.length === 0
      ? of([] as CalendarEventDto[])
      : this.calendarService.getEvents(fromDate, toDate, this.scope, selectedTypes).pipe(
        timeout(30000),
        catchError(err => {
          console.error('Calendar events load failed', err);
          this.notificationService.error(
            this.notificationService.getMessage(err, 'Nem sikerült betölteni a naptár eseményeit.')
          );
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

        this.dayInfos = this.enrichHungarianHolidayDayInfos(dayInfos);
        this.events = events.map(event => this.normalizeEvent(event));
        this.rebuildView();
      }),
      catchError(err => {
        if (currentLoadVersion !== this.loadVersion) {
          return of(undefined);
        }

        console.error('Unexpected calendar load error', err);
        this.showRetryableError('Váratlan hiba történt a naptár betöltése közben.');
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
    const dayInfoMap = new Map(this.dayInfos.map(dayInfo => [dayInfo.date.substring(0, 10), dayInfo]));
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
    if (this.viewMode === 'year') {
      return {
        from: new Date(this.currentDate.getFullYear(), 0, 1),
        to: new Date(this.currentDate.getFullYear(), 11, 31)
      };
    }

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

    return this.enrichHungarianHolidayDayInfos(result);
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

  private enrichHungarianHolidayDayInfos(dayInfos: CalendarDayInfoDto[]): CalendarDayInfoDto[] {
    if (!dayInfos.length) {
      return dayInfos;
    }

    const holidayFallbacks = this.createHungarianHolidayFallbackMap(dayInfos);

    return dayInfos.map(dayInfo => {
      const dateKey = dayInfo.date.substring(0, 10);
      const fallbackHolidayName = holidayFallbacks.get(dateKey);

      if (!fallbackHolidayName || dayInfo.isHoliday) {
        return {
          ...dayInfo,
          date: dateKey
        };
      }

      return {
        ...dayInfo,
        date: dateKey,
        isHoliday: true,
        isWorkingDay: false,
        holidayName: dayInfo.holidayName || fallbackHolidayName
      };
    });
  }

  private createHungarianHolidayFallbackMap(dayInfos: CalendarDayInfoDto[]): Map<string, string> {
    const result = new Map<string, string>();
    const years = [
      ...new Set(
        dayInfos
          .map(dayInfo => Number(dayInfo.date.substring(0, 4)))
          .filter(year => !Number.isNaN(year))
      )
    ];

    years.forEach(year => {
      this.addFixedHoliday(result, year, 1, 1, 'Újév');
      this.addFixedHoliday(result, year, 3, 15, 'Nemzeti ünnep');
      this.addFixedHoliday(result, year, 5, 1, 'A munka ünnepe');
      this.addFixedHoliday(result, year, 8, 20, 'Államalapítás ünnepe');
      this.addFixedHoliday(result, year, 10, 23, 'Nemzeti ünnep');
      this.addFixedHoliday(result, year, 11, 1, 'Mindenszentek');
      this.addFixedHoliday(result, year, 12, 25, 'Karácsony');
      this.addFixedHoliday(result, year, 12, 26, 'Karácsony másnapja');

      const easterSunday = this.getEasterSunday(year);

      this.addMovingHoliday(result, easterSunday, -2, 'Nagypéntek');
      this.addMovingHoliday(result, easterSunday, 0, 'Húsvétvasárnap');
      this.addMovingHoliday(result, easterSunday, 1, 'Húsvéthétfő');
      this.addMovingHoliday(result, easterSunday, 49, 'Pünkösdvasárnap');
      this.addMovingHoliday(result, easterSunday, 50, 'Pünkösdhétfő');
    });

    return result;
  }

  private addFixedHoliday(
    result: Map<string, string>,
    year: number,
    month: number,
    day: number,
    holidayName: string
  ): void {
    result.set(this.formatDateForApi(new Date(year, month - 1, day)), holidayName);
  }

  private addMovingHoliday(
    result: Map<string, string>,
    baseDate: Date,
    offsetDays: number,
    holidayName: string
  ): void {
    const date = new Date(baseDate);
    date.setDate(date.getDate() + offsetDays);
    result.set(this.formatDateForApi(date), holidayName);
  }

  private getEasterSunday(year: number): Date {
    const a = year % 19;
    const b = Math.floor(year / 100);
    const c = year % 100;
    const d = Math.floor(b / 4);
    const e = b % 4;
    const f = Math.floor((b + 8) / 25);
    const g = Math.floor((b - f + 1) / 3);
    const h = (19 * a + b - d - g + 15) % 30;
    const i = Math.floor(c / 4);
    const k = c % 4;
    const l = (32 + 2 * e + 2 * i - h - k) % 7;
    const m = Math.floor((a + 11 * h + 22 * l) / 451);
    const month = Math.floor((h + l - 7 * m + 114) / 31);
    const day = ((h + l - 7 * m + 114) % 31) + 1;

    return new Date(year, month - 1, day);
  }

  private storeNavigationContext(event: CalendarEventDto): void {
    if (event.type !== 'deskBooking') {
      return;
    }

    try {
      const storageKey = 'desk-booking-state';
      const rawState = localStorage.getItem(storageKey);
      const existingState = rawState ? JSON.parse(rawState) : {};

      localStorage.setItem(storageKey, JSON.stringify({
        selectedLocationId: existingState.selectedLocationId ?? '',
        selectedOfficeId: existingState.selectedOfficeId ?? '',
        selectedWorkstationId: existingState.selectedWorkstationId ?? '',
        selectedDate: event.dateFrom
      }));
    } catch (error) {
      console.warn('Nem sikerült eltárolni a helyfoglalás navigációs állapotát.', error);
    }
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

  private showRetryableError(message: string): void {
    this.notificationService
      .error(message, {
        actionLabel: 'Újrapróbálás',
        durationMs: 8000
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadCalendar();
      });
  }

  private getRequestValidationMessage(): string | null {
    if (!this.requestForm.type) {
      return 'Válassz igénytípust.';
    }

    if (!this.requestForm.dateFrom || !this.requestForm.dateTo) {
      return 'Add meg a kezdő és záró dátumot.';
    }

    if (this.requestForm.dateTo < this.requestForm.dateFrom) {
      return 'A záró dátum nem lehet korábbi, mint a kezdő dátum.';
    }

    if (this.requestForm.dateFrom < this.formatDateForApi(new Date())) {
      return 'Múltbeli napra nem lehet új igényt rögzíteni.';
    }

    if (this.hasActiveAbsenceRequestInRange(
      this.requestForm.dateFrom,
      this.requestForm.dateTo
    )) {
      return 'A megadott időszakban már van aktív kérelmed.';
    }

    return null;
  }

  private hasActiveAbsenceRequestInRange(dateFrom: string, dateTo: string): boolean {
    if (this.scope !== 'mine') {
      return false;
    }

    return this.events.some(event => {
      const status = this.normalizeStatus(event.status);

      return event.type !== 'deskBooking'
        && status !== 'cancelled'
        && status !== 'rejected'
        && event.dateFrom <= dateTo
        && event.dateTo >= dateFrom;
    });
  }
}