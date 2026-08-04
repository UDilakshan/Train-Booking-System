import { Component } from '@angular/core';
import { SeatComponent } from './seat.component';

@Component({
  selector: 'app-seat-map-legend',
  standalone: true,
  imports: [SeatComponent],
  template: `
    <div class="legend">
      <div class="item"><app-seat state="available" /> <span>Available</span></div>
      <div class="item"><app-seat state="selected" /> <span>Selected</span></div>
      <div class="item"><app-seat state="occupied" /> <span>Occupied</span></div>
      <div class="item"><app-seat state="unavailable" /> <span>Unavailable</span></div>
    </div>
  `,
  styles: [
    `
      .legend {
        display: flex;
        flex-wrap: wrap;
        gap: 1.25rem;
        margin-bottom: 1rem;
      }
      .item {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        font-size: 0.85rem;
        color: rgba(0, 0, 0, 0.7);
      }
    `,
  ],
})
export class SeatMapLegendComponent {}
