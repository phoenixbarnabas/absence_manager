export interface CreateLocationDto {
  name: string;
  displayOrder: number;
}

export interface LocationViewDto {
  id: number;
  name: string;
  isActive: boolean;
  displayOrder: number;
}

export interface UpdateLocationDto {
  id: number;
  name: string;
  isActive: boolean;
  displayOrder: number;
}