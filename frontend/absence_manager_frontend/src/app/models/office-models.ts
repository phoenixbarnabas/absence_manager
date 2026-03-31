export interface OfficeViewDto {
  id: string;
  locationId: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  displayOrder: number;
}