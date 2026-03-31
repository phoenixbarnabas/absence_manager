export interface OfficeDayAvailabilityDto {
  bookingDate: string;
  locationId: string;
  locationName: string;
  officeId: string;
  officeName: string;
  totalWorkstations: number;
  bookedWorkstations: number;
  freeWorkstations: number;
  currentUserHasBooking: boolean;
  currentUserBookingId?: string | null;
  currentUserWorkstationId?: string | null;
  workstations: WorkstationAvailabilityDto[];
}

export interface WorkstationAvailabilityDto {
  workstationId: string;
  workstationCode: string;
  workstationName: string;
  displayOrder: number;
  isActive: boolean;
  isBooked: boolean;
  isBookedByCurrentUser: boolean;
  bookedByUserId?: string | null;
  bookedByDisplayName?: string | null;
  positionX?: number | null;
  positionY?: number | null;
  bookingId?: string | null;
  bookedByUserName?: string | null;
}