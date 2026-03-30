export interface CancelOfficeBookingDto {
  bookingId: string;
}

export interface CreateOfficeBookingDto {
  workstationId: string;
  bookingDate: string;
}

export interface OfficeBookingViewDto {
  id: string;
  bookingDate: string;
  userId: string;
  userName: string;
  workstationId: string;
  workstationCode: string;
  workstationName: string;
  officeId: string;
  officeName: string;
  locationId: string;
  locationName: string;
  isCancelled: boolean;
  createdAtUtc: string;
}

export interface DaySummaryDto {
  date: Date
  totalWorkstations: number
  bookedWorkstations: number
  freeWorkstations: number
  currentUserHasBooking: boolean
}