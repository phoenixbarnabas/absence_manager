import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Workstation } from '../../models/entity-models';
import { WorkstationAvailabilityDto } from '../../models/availability-models';

@Component({
  selector: 'app-workstation-list',
  standalone: false,
  templateUrl: './workstation-list.html',
  styleUrl: './workstation-list.sass',
})
export class WorkstationList {
  @Input() workstations: WorkstationAvailabilityDto[] = [];
  @Input() selectedWorkstationId: string = '';
  @Input() currentUserHasBooking: boolean = false;

  @Output() workstationSelected = new EventEmitter<string>();

  selectWorkstation(workstationId: string): void {
    this.workstationSelected.emit(workstationId);
  }

  isSelected(workstationId: string): boolean {
    return this.selectedWorkstationId === workstationId;
  }

  isSelectable(workstation: WorkstationAvailabilityDto): boolean {
    if (!workstation.isActive) {
      return false;
    }

    if (workstation.isBooked && !workstation.isBookedByCurrentUser) {
      return false;
    }

    return true;
  }
}
