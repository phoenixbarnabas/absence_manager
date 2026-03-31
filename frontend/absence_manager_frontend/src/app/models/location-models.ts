export interface CreateLocationDto {
  name: string;
  displayOrder: number;
}

export interface LocationViewDto {
  id: string;
  name: string;
  isActive: boolean;
  displayOrder: number;
}

export interface UpdateLocationDto {
  id: string;
  name: string;
  isActive: boolean;
  displayOrder: number;
}