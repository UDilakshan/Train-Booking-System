import { Component, computed, input, output } from '@angular/core';

export type SeatState = 'available' | 'occupied' | 'selected' | 'unavailable';

@Component({
  selector: 'app-seat',
  standalone: true,
  template: `
    <button
      type="button"
      class="seat"
      [class]="state()"
      [disabled]="state() === 'occupied' || state() === 'unavailable'"
      [attr.aria-label]="seatNumber() ? 'Seat ' + seatNumber() + ' — ' + state() : 'Seat  — ' + state()"
      (click)="seatClick.emit()"
    >
      {{ seatNumber() }}
    </button>
  `,
  styles: [
    `
      .seat {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 2.5rem;
        height: 2.5rem;
        border-radius: 0.375rem;
        border: 1px solid rgba(0, 0, 0, 0.2);
        background: #fff;
        font-size: 0.8rem;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.15s ease;
      }
      .seat:hover:not(:disabled) {
        border-color: #1a4d8f;
        transform: translateY(-1px);
      }
      .seat.available {
        background: #ffffff;
        color: #1a1a1a;
      }
      .seat.selected {
        background: #1a4d8f;
        border-color: #1a4d8f;
        color: #fff;
      }
      .seat.occupied {
        background: #fdecea;
        border-color: #f3c4bd;
        color: #b3261e;
        cursor: not-allowed;
      }
      .seat.unavailable {
        background: #f1f1f1;
        border-color: #e0e0e0;
        color: #9e9e9e;
        cursor: not-allowed;
      }
    `,
  ],
})
export class SeatComponent {
  readonly seatNumber = input<string>('');
  readonly state = input.required<SeatState>();
  readonly seatClick = output<void>();

  protected readonly stateLabel = computed(() => this.state());
}
