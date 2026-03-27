import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

type DeskStatus = 'free' | 'selected' | 'occupied';

type Desk = {
  id: number;
  name: string;
  status: DeskStatus;
}

type Office = {
  id: number;
  name: string;
  desks: Desk[];
}

type Location = {
  id: number;
  name: string;
  offices: Office[];
}

type CalendarDay = {
  date: Date;
  dayLabel: string;
  dayNumber: number;
  isSelected: boolean;
}

@Component({
  selector: 'app-desk-booking',
  standalone: false,
  templateUrl: './desk-booking.html',
  styleUrl: './desk-booking.sass',
})
export class DeskBooking implements OnInit {
  calendarDays: CalendarDay[] = []

  locations: Location[] = [
    {
      id: 1,
      name: 'Fót',
      offices: [
        {
          id: 1,
          name: '1. iroda',
          desks: [
            { id: 1, name: '1. asztal', status: 'free' },
            { id: 2, name: '2. asztal', status: 'free' },
            { id: 3, name: '3. asztal', status: 'occupied' },
            { id: 4, name: '4. asztal', status: 'free' },
            { id: 5, name: '5. asztal', status: 'free' },
            { id: 6, name: '6. asztal', status: 'occupied' }
          ]
        },
        {
          id: 2,
          name: '2. iroda',
          desks: [
            { id: 1, name: '1. asztal', status: 'free' },
            { id: 2, name: '2. asztal', status: 'free' },
            { id: 3, name: '3. asztal', status: 'free' },
            { id: 4, name: '4. asztal', status: 'occupied' }
          ]
        }
      ]
    },
    {
      id: 2,
      name: 'Budapest',
      offices: [
        {
          id: 3,
          name: 'A iroda',
          desks: [
            { id: 1, name: '1. asztal', status: 'free' },
            { id: 2, name: '2. asztal', status: 'occupied' },
            { id: 3, name: '3. asztal', status: 'free' },
            { id: 4, name: '4. asztal', status: 'free' },
            { id: 5, name: '5. asztal', status: 'free' }
          ]
        },
        {
          id: 4,
          name: 'B iroda',
          desks: [
            { id: 1, name: '1. asztal', status: 'free' },
            { id: 2, name: '2. asztal', status: 'occupied' }
          ]
        }
      ]
    }
  ]

  selectedLocationId!: number
  selectedOfficeId!: number

  ngOnInit(): void {
    this.generateCalendarDays()
    this.selectedLocationId = this.locations[0].id
    this.selectedOfficeId = this.currentLocation.offices[0].id
  }

  get currentLocation(): Location {
    return this.locations.find(location => location.id === this.selectedLocationId)!
  }

  get currentOffice(): Office {
    return this.currentLocation.offices.find(office => office.id === this.selectedOfficeId)!
  }

  get offices(): Office[] {
    return this.currentLocation.offices
  }

  get desks(): Desk[] {
    return this.currentOffice.desks
  }

  onLocationChange(): void {
    this.selectedOfficeId = this.currentLocation.offices[0].id
  }

  selectDay(selectedDay: CalendarDay): void {
    this.calendarDays = this.calendarDays.map(day => ({
      ...day,
      isSelected: day.date.getTime() === selectedDay.date.getTime()
    }))
  }

  selectDesk(desk: Desk): void {
    if (desk.status === 'occupied') {
      return
    }

    this.currentOffice.desks = this.currentOffice.desks.map(item => {
      if (item.status === 'selected') {
        return { ...item, status: 'free' }
      }

      return item;
    })

    const clickedDesk = this.currentOffice.desks.find(item => item.id === desk.id);
    if (!clickedDesk) {
      return;
    }

    clickedDesk.status = clickedDesk.status === 'selected' ? 'free' : 'selected'
  }

  private generateCalendarDays(): void {
    const dayNames = ['V', 'H', 'K', 'Sze', 'Cs', 'P', 'Szo']
    const today = new Date()

    this.calendarDays = Array.from({ length: 14 }, (_, index) => {
      const date = new Date(today)
      date.setDate(today.getDate() + index)

      return {
        date,
        dayLabel: dayNames[date.getDay()],
        dayNumber: date.getDate(),
        isSelected: index === 0
      };
    });
  }
}
