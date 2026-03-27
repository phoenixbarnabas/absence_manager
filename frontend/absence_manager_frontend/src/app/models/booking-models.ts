export interface CancelOfficeBookingDto {
  bookingId: number;
}

export interface CreateOfficeBookingDto {
  workstationId: string;
  bookingDate: string;
}

export interface OfficeBookingViewDto {
  id: number;
  bookingDate: string;
  userId: string;
  userName: string;
  workstationId: number;
  workstationCode: string;
  workstationName: string;
  officeId: number;
  officeName: string;
  locationId: number;
  locationName: string;
  isCancelled: boolean;
  createdAtUtc: string;
}