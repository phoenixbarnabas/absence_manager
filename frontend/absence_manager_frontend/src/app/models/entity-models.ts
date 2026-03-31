export interface AppUser {
  id: string;
  entraObjectId: string;
  tenantId?: string | null;
  displayName: string;
  email?: string | null;
  isActive: boolean;
  createdAt: string;
  officeBookings: OfficeBooking[];
}

export interface Location {
  id: string;
  name: string;
  isActive: boolean;
  displayOrder: number;
  offices: Office[];
}

export interface Office {
  id: string;
  locationId: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  displayOrder: number;
  location: Location;
  workstations: Workstation[];
}

export interface OfficeBooking {
  id: string;
  workstationId: string;
  userId: string;
  bookingDate: string;
  createdAtUtc: string;
  createdByUserId: string;
  cancelledAtUtc?: string | null;
  cancelledByUserId?: string | null;
  isCancelled: boolean;
  workstation: Workstation;
  user: AppUser;
}

export interface Workstation {
  id: string;
  officeId: string;
  code: string;
  name: string;
  isActive: boolean;
  displayOrder: number;
  positionX?: number | null;
  positionY?: number | null;
  office: Office;
  bookings: OfficeBooking[];
}
