export type CalendarViewMode = 'month' | 'week';

export type CalendarScope = 'mine' | 'team' | 'organization';

export type CalendarEventType =
  | 'vacation'
  | 'homeOffice'
  | 'sickLeave'
  | 'otherAbsence'
  | 'deskBooking';

export type CalendarAbsenceRequestType =
  | 'vacation'
  | 'homeOffice'
  | 'sickLeave'
  | 'otherAbsence';

export type CalendarEventStatus =
  | 'approved'
  | 'pending'
  | 'rejected'
  | 'cancelled'
  | 'info';

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
  department?: string | null;
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
  dayName: string;
  isToday: boolean;
  isCurrentMonth: boolean;
  isSelected: boolean;
  isWeekend: boolean;
  isHoliday: boolean;
  isWorkingDay: boolean;
  holidayName?: string | null;
  events: CalendarEventDto[];
}

export interface CreateAbsenceRequestDto {
  type: CalendarAbsenceRequestType;
  dateFrom: string;
  dateTo: string;
  reason?: string | null;
}

export interface AbsenceRequestViewDto {
  id: string;
  type: CalendarAbsenceRequestType;
  status: CalendarEventStatus;
  dateFrom: string;
  dateTo: string;
  reason?: string | null;
  userId: string;
  userName: string;
  department?: string | null;
  createdAtUtc: string;
}