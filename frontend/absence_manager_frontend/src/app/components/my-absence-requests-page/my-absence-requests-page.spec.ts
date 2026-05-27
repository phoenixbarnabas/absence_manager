import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyAbsenceRequestsPage } from './my-absence-requests-page';

describe('MyAbsenceRequestsPage', () => {
  let component: MyAbsenceRequestsPage;
  let fixture: ComponentFixture<MyAbsenceRequestsPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MyAbsenceRequestsPage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyAbsenceRequestsPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
