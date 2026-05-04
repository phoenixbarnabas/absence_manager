export type CalendarViewMode = 'month' | 'week' | 'year';
export type CalendarScope = 'mine' | 'team' | 'organization';
export type CalendarEventType = 'vacation' | 'homeOffice' | 'sickLeave' | 'otherAbsence' | 'deskBooking';
export type CalendarEventStatus = 'approved' | 'pending' | 'rejected' | 'cancelled' | 'info';

export interface CalendarDayInfoDto {
  date: string;
  isWeekend: boolean;
  isHoliday: boolean;
  isWorkingDay: boolean;
  holidayName?: string | null;
}

export interface CalendarEventDto {
  id: string;
  title: string;
  type: CalendarEventType;
  status: CalendarEventStatus;
  dateFrom: string;
  dateTo: string;
  userId: string;
  userName: string;
  description?: string | null;
  sourceId?: string | null;
  detailsUrl?: string | null;
  locationName?: string | null;
  officeName?: string | null;
  workstationName?: string | null;
}

export interface CalendarDayView {
  date: Date;
  dateKey: string;
  dayNumber: number;
  isToday: boolean;
  isCurrentMonth: boolean;
  isSelected: boolean;
  isWeekend: boolean;
  isHoliday: boolean;
  isWorkingDay: boolean;
  holidayName?: string | null;
  events: CalendarEventDto[];
}

export interface CalendarMonthSummary {
  monthIndex: number;
  monthName: string;
  totalEvents: number;
  workdays: number;
  holidays: number;
  eventsByType: Record<CalendarEventType, number>;
}
