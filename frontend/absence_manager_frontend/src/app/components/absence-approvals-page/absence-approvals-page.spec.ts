import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AbsenceApprovalsPage } from './absence-approvals-page';

describe('AbsenceApprovalsPage', () => {
  let component: AbsenceApprovalsPage;
  let fixture: ComponentFixture<AbsenceApprovalsPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AbsenceApprovalsPage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AbsenceApprovalsPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
