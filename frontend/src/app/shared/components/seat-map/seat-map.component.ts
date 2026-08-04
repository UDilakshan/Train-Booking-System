import { Component, computed, input, output } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { SeatAvailability } from '../../../core/models/availability.model';
import { CoachType } from '../../../core/models/train.model';
import { SeatMapLegendComponent } from './seat-map-legend.component';
import { SeatComponent, SeatState } from './seat.component';

interface CoachGroup {
  coachId: string;
  coachNumber: string;
  coachType: CoachType;
  seats: SeatAvailability[];
}

const COACH_TYPE_LABELS: Record<CoachType, string> = {
  FirstClass: 'First Class',
  SecondClass: 'Second Class',
  ThirdClass: 'Third Class',
  Observation: 'Observation',
};

@Component({
  selector: 'app-seat-map',
  standalone: true,
  imports: [MatCardModule, SeatComponent, SeatMapLegendComponent],
  template: `
    <app-seat-map-legend />
    @for (coach of coachGroups(); track coach.coachId) {
      <mat-card class="coach-card">
        <mat-card-header>
          <mat-card-title>Coach {{ coach.coachNumber }} — {{ coachTypeLabel(coach.coachType) }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="seat-grid">
            @for (seat of coach.seats; track seat.seatId) {
              <app-seat
                [seatNumber]="seat.seatNumber"
                [state]="seatState(seat)"
                (seatClick)="toggle(seat)"
              />
            }
          </div>
        </mat-card-content>
      </mat-card>
    }
  `,
  styles: [
    `
      .coach-card {
        margin-bottom: 1rem;
      }
      .seat-grid {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
      }
    `,
  ],
})
export class SeatMapComponent {
  readonly seats = input.required<SeatAvailability[]>();
  readonly selectedSeatIds = input.required<ReadonlySet<string>>();
  readonly seatToggle = output<SeatAvailability>();

  protected readonly coachGroups = computed<CoachGroup[]>(() => {
    const groups = new Map<string, CoachGroup>();
    for (const seat of this.seats()) {
      let group = groups.get(seat.coachId);
      if (!group) {
        group = { coachId: seat.coachId, coachNumber: seat.coachNumber, coachType: seat.coachType, seats: [] };
        groups.set(seat.coachId, group);
      }
      group.seats.push(seat);
    }
    return [...groups.values()].sort((a, b) => a.coachNumber.localeCompare(b.coachNumber));
  });

  protected coachTypeLabel(type: CoachType): string {
    return COACH_TYPE_LABELS[type];
  }

  protected seatState(seat: SeatAvailability): SeatState {
    if (this.selectedSeatIds().has(seat.seatId)) return 'selected';
    return seat.isAvailable ? 'available' : 'occupied';
  }

  protected toggle(seat: SeatAvailability): void {
    if (!seat.isAvailable) return;
    this.seatToggle.emit(seat);
  }
}
