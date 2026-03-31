import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeskBooking } from './desk-booking';

describe('DeskBooking', () => {
  let component: DeskBooking;
  let fixture: ComponentFixture<DeskBooking>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DeskBooking]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeskBooking);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
