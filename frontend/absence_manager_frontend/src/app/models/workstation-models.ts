export interface WorkstationViewDto {
  id: string;
  officeId: string;
  code: string;
  name: string;
  isActive: boolean;
  displayOrder: number;
  positionX?: number | null;
  positionY?: number | null;
}