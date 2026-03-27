export interface WorkstationViewDto {
  id: number;
  officeId: number;
  code: string;
  name: string;
  isActive: boolean;
  displayOrder: number;
  positionX?: number | null;
  positionY?: number | null;
}