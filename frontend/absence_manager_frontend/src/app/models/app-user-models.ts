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