import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, finalize, forkJoin, of } from 'rxjs';


@Component({
  selector: 'app-calendar-page',
  standalone: false,
  templateUrl: './calendar-page.html',
  styleUrl: './calendar-page.sass'
})
export class CalendarPage implements OnInit {
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
    { value: 'week', label: 'Heti', icon: 'bi-calendar-week' },
    { value: 'year', label: 'Éves', icon: 'bi-calendar4-range' }
  ];

  readonly scopeOptions: { value: CalendarScope; label: string; description: string }[] = [
    { value: 'mine', label: 'Saját', description: 'Csak a saját eseményeim' },
    { value: 'team', label: 'Csapat', description: 'Managerként a saját csapat eseményei' },
    { value: 'organization', label: 'Szervezet', description: 'HR/Admin jogosultsággal szélesebb nézet' }
  ];

  readonly eventTypeOptions: { value: CalendarEventType; label: string; icon: string }[] = [
    { value: 'vacation', label: 'Szabadság', icon: 'bi-suitcase-lg' },
    { value: 'homeOffice', label: 'Home office', icon: 'bi-house-door' },
    { value: 'sickLeave', label: 'Betegszabadság', icon: 'bi-heart-pulse' },
    { value: 'otherAbsence', label: 'Egyéb távollét', icon: 'bi-calendar-x' },
    { value: 'deskBooking', label: 'Helyfoglalás', icon: 'bi-grid-3x2-gap' }
  ];

  viewMode: CalendarViewMode = 'month';
  scope: CalendarScope = 'mine';
  currentDate = this.startOfDay(new Date());
  selectedDateKey = this.formatDateForApi(new Date());

  calendarDays: CalendarDayView[] = [];
  monthSummaries: CalendarMonthSummary[] = [];
  selectedEvent: CalendarEventDto | null = null;

  eventTypeSelection: Record<CalendarEventType, boolean> = {
    vacation: true,
    homeOffice: true,
    sickLeave: true,
    otherAbsence: true,
    deskBooking: true
  };

  loading = false;
  errorMessage = '';
  warningMessage = '';

  private dayInfos: CalendarDayInfoDto[] = [];
  private events: CalendarEventDto[] = [];
  private rangeFrom = this.currentDate;
  private rangeTo = this.currentDate;

  constructor(
    private calendarService: CalendarService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadCalendar();
  }

  get periodTitle(): string {
    if (this.viewMode === 'year') {
      return `${this.currentDate.getFullYear()}`;
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

  setViewMode(mode: CalendarViewMode): void {
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
      this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() - 1, 1);
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
      this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() + 1, 1);
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
    this.calendarDays = this.calendarDays.map(calendarDay => ({
      ...calendarDay,
      isSelected: calendarDay.dateKey === day.dateKey
    }));
  }

  selectEvent(event: CalendarEventDto, domEvent?: Event): void {
    domEvent?.stopPropagation();
    this.selectedEvent = event;
    this.selectedDateKey = event.dateFrom;
    this.calendarDays = this.calendarDays.map(calendarDay => ({
      ...calendarDay,
      isSelected: calendarDay.dateKey === event.dateFrom
    }));
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

  trackByMonth(_: number, summary: CalendarMonthSummary): number {
    return summary.monthIndex;
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

  hasMonthSummaryEvent(summary: CalendarMonthSummary, type: CalendarEventType): boolean {
    return summary.eventsByType[type] > 0;
  }

  private loadCalendar(): void {
    const range = this.getCurrentRange();
    this.rangeFrom = range.from;
    this.rangeTo = range.to;

    const fromDate = this.formatDateForApi(range.from);
    const toDate = this.formatDateForApi(range.to);
    const selectedTypes = this.activeEventTypes;

    this.loading = true;
    this.errorMessage = '';
    this.warningMessage = '';

    const dayInfos$ = this.calendarService.getDayInfos(fromDate, toDate).pipe(
      catchError(err => {
        console.error(err);
        this.warningMessage = 'A munkanap/ünnepnap API nem érhető el, ezért ideiglenes helyi naptárlogikát használok.';
        return of(this.generateFallbackDayInfos(range.from, range.to));
      })
    );

    const events$ = this.calendarService.getEvents(fromDate, toDate, this.scope, selectedTypes).pipe(
      catchError(err => {
        console.error(err);
        this.errorMessage = err?.status === 403
          ? 'Ehhez a naptár nézethez nincs megfelelő jogosultságod.'
          : 'Nem sikerült betölteni a naptár eseményeit.';
        return of([] as CalendarEventDto[]);
      })
    );

    forkJoin({ dayInfos: dayInfos$, events: events$ })
      .pipe(finalize(() => this.loading = false))
      .subscribe(({ dayInfos, events }) => {
        this.dayInfos = dayInfos;
        this.events = events;
        this.rebuildView();
      });
  }

  private rebuildView(): void {
    const dayInfoMap = new Map(this.dayInfos.map(dayInfo => [dayInfo.date, dayInfo]));
    const days: CalendarDayView[] = [];

    for (let date = new Date(this.rangeFrom); date <= this.rangeTo; date.setDate(date.getDate() + 1)) {
      const currentDate = new Date(date);
      const dateKey = this.formatDateForApi(currentDate);
      const dayInfo = dayInfoMap.get(dateKey) ?? this.generateFallbackDayInfo(currentDate);

      days.push({
        date: currentDate,
        dateKey,
        dayNumber: currentDate.getDate(),
        isToday: this.isSameDate(currentDate, new Date()),
        isCurrentMonth: this.viewMode !== 'month' || currentDate.getMonth() === this.currentDate.getMonth(),
        isSelected: dateKey === this.selectedDateKey,
        isWeekend: dayInfo.isWeekend,
        isHoliday: dayInfo.isHoliday,
        isWorkingDay: dayInfo.isWorkingDay,
        holidayName: dayInfo.holidayName,
        events: this.events.filter(event => this.isEventOnDate(event, dateKey))
      });
    }

    this.calendarDays = days;
    this.monthSummaries = this.buildMonthSummaries();

    if (this.selectedEvent && !this.events.some(event => event.id === this.selectedEvent?.id)) {
      this.selectedEvent = null;
    }
  }

  private buildMonthSummaries(): CalendarMonthSummary[] {
    const year = this.currentDate.getFullYear();

    return this.monthNames.map((monthName, monthIndex) => {
      const firstDay = new Date(year, monthIndex, 1);
      const lastDay = new Date(year, monthIndex + 1, 0);
      const counters = this.createEmptyEventCounter();
      const monthEvents = this.events.filter(event => this.eventIntersectsRange(event, firstDay, lastDay));
      const monthDayInfos = this.dayInfos.filter(dayInfo => this.parseDateKey(dayInfo.date).getMonth() === monthIndex);

      monthEvents.forEach(event => {
        counters[event.type] += 1;
      });

      return {
        monthIndex,
        monthName,
        totalEvents: monthEvents.length,
        workdays: monthDayInfos.filter(dayInfo => dayInfo.isWorkingDay).length,
        holidays: monthDayInfos.filter(dayInfo => dayInfo.isHoliday).length,
        eventsByType: counters
      };
    });
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

      return { from: weekStart, to: weekEnd };
    }

    const firstDayOfMonth = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth(), 1);
    const lastDayOfMonth = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() + 1, 0);
    const gridStart = this.getStartOfWeek(firstDayOfMonth);
    const gridEnd = this.getEndOfWeek(lastDayOfMonth);

    return { from: gridStart, to: gridEnd };
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

  private isEventOnDate(event: CalendarEventDto, dateKey: string): boolean {
    return event.dateFrom <= dateKey && event.dateTo >= dateKey;
  }

  private eventIntersectsRange(event: CalendarEventDto, from: Date, to: Date): boolean {
    const fromKey = this.formatDateForApi(from);
    const toKey = this.formatDateForApi(to);
    return event.dateFrom <= toKey && event.dateTo >= fromKey;
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

  private createEmptyEventCounter(): Record<CalendarEventType, number> {
    return {
      vacation: 0,
      homeOffice: 0,
      sickLeave: 0,
      otherAbsence: 0,
      deskBooking: 0
    };
  }

  private formatDateForApi(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  private parseDateKey(dateKey: string): Date {
    const [year, month, day] = dateKey.split('-').map(Number);
    return new Date(year, month - 1, day);
  }

  private formatShortDate(date: Date): string {
    return `${this.monthNames[date.getMonth()].slice(0, 3)} ${date.getDate()}.`;
  }

  private formatLongDate(date: Date): string {
    return `${date.getFullYear()}. ${this.monthNames[date.getMonth()]} ${date.getDate()}.`;
  }

  private startOfDay(date: Date): Date {
    return new Date(date.getFullYear(), date.getMonth(), date.getDate());
  }

  private isSameDate(first: Date, second: Date): boolean {
    return first.getFullYear() === second.getFullYear()
      && first.getMonth() === second.getMonth()
      && first.getDate() === second.getDate();
  }
}import { CalendarDayInfoDto, CalendarDayView, CalendarEventDto, CalendarEventType, CalendarMonthSummary, CalendarScope, CalendarViewMode } from '../../models/calendar-models';
import { CalendarService } from '../../services/calendar-service';

