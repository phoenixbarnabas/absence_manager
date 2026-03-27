export interface OfficeViewDto {
  id: number;
  locationId: number;
  name: string;
  description?: string | null;
  isActive: boolean;
  displayOrder: number;
}