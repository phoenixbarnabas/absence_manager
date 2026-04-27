import { OfficeBooking } from "./entity-models";

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

export interface UserProfile {
  displayName: string;
  email: string;
  department: string;
  jobTitle: string;
}