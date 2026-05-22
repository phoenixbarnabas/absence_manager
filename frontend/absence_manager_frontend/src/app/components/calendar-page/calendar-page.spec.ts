import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { CalendarPage } from './calendar-page';
import { CalendarService } from '../../../services/calendar-service';

class CalendarServiceStub {
  getDayInfos() {
    return of([]);
  }

  getEvents() {
    return of([]);
  }
}

describe('CalendarPage', () => {
  let component: CalendarPage;
  let fixture: ComponentFixture<CalendarPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CalendarPage],
      imports: [RouterTestingModule, FormsModule],
      providers: [
        { provide: CalendarService, useClass: CalendarServiceStub }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CalendarPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
