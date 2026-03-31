import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkstationList } from './workstation-list';

describe('WorkstationList', () => {
  let component: WorkstationList;
  let fixture: ComponentFixture<WorkstationList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [WorkstationList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WorkstationList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
