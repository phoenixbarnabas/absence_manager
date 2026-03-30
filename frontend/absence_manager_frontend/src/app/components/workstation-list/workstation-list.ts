import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Workstation } from '../../models/entity-models';

@Component({
  selector: 'app-workstation-list',
  standalone: false,
  templateUrl: './workstation-list.html',
  styleUrl: './workstation-list.sass',
})
export class WorkstationList {
  @Input() workstations: Workstation[] = [];
  @Input() selectedWorkstationId: string = '';

  @Output() workstationSelected = new EventEmitter<string>();

  selectWorkstation(workstationId: string): void {
    this.workstationSelected.emit(workstationId);
  }

  isSelected(workstationId: string): boolean {
    return this.selectedWorkstationId === workstationId;
  }

  get visibleWorkstations(): Workstation[] {
    return this.workstations
      .filter(workstation => workstation.isActive)
      .sort((a, b) => a.displayOrder - b.displayOrder);
  }
}
