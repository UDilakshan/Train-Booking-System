import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiError } from '../../core/models/api-response.model';
import { Booking } from '../../core/models/booking.model';
import { BookingsService } from '../../core/services/bookings.service';

@Component({
  selector: 'app-booking-confirmation',
  standalone: true,
  imports: [RouterLink, DecimalPipe, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule, MatProgressSpinnerModule],
  template: `
    <div class="container">
      @if (loading()) {
        <div class="loading"><mat-spinner diameter="32" /></div>
      } @else if (error()) {
        <p class="error">{{ error() }}</p>
      } @else if (booking(); as booking) {
        <div class="confirmed-icon">
          <mat-icon color="primary">check_circle</mat-icon>
        </div>
        <h1>Booking Confirmed</h1>
        <p class="subtitle">Keep your reference number for check-in and future changes.</p>

        <mat-card class="ticket-card">
          <mat-card-content>
            <div class="ref-row">
              <span class="reference">{{ booking.bookingReference }}</span>
              <mat-chip-set><mat-chip color="primary" highlighted>{{ booking.status }}</mat-chip></mat-chip-set>
            </div>

            <div class="grid">
              <div><span class="label">Passenger</span><br /><strong>{{ booking.passengerName }}</strong></div>
              <div><span class="label">Contact</span><br /><strong>{{ booking.passengerContact }}</strong></div>
              <div><span class="label">Train</span><br /><strong>{{ booking.trainName }}</strong></div>
              <div><span class="label">Journey date</span><br /><strong>{{ booking.journeyDate }} &middot; {{ booking.departureTime }}</strong></div>
              <div><span class="label">From</span><br /><strong>{{ booking.originStationName }}</strong></div>
              <div><span class="label">To</span><br /><strong>{{ booking.destinationStationName }}</strong></div>
            </div>

            <table class="seats-table">
              <thead><tr><th>Coach</th><th>Seat</th><th>Fare</th></tr></thead>
              <tbody>
                @for (segment of booking.segments; track segment.id) {
                  <tr>
                    <td>{{ segment.coachNumber }}</td>
                    <td>{{ segment.seatNumber }}</td>
                    <td>LKR {{ segment.fare | number: '1.2-2' }}</td>
                  </tr>
                }
              </tbody>
            </table>

            <div class="total">
              <span>Total Paid</span>
              <strong>LKR {{ booking.totalFare | number: '1.2-2' }}</strong>
            </div>

            <div class="actions">
              <a mat-stroked-button [routerLink]="['/my-bookings']" [queryParams]="{ reference: booking.bookingReference }">View in My Bookings</a>
              <a mat-raised-button color="primary" routerLink="/search">Book Another Journey</a>
            </div>
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [
    `
      .container {
        max-width: 700px;
        margin: 0 auto;
        padding: 2.5rem 1.5rem;
        text-align: center;
      }
      .loading {
        display: flex;
        justify-content: center;
        padding: 3rem 0;
      }
      .error {
        color: #b3261e;
      }
      .confirmed-icon mat-icon {
        font-size: 3rem;
        width: 3rem;
        height: 3rem;
      }
      h1 {
        margin: 0.5rem 0 0.25rem;
      }
      .subtitle {
        color: rgba(0, 0, 0, 0.6);
        margin: 0 0 1.5rem;
      }
      .ticket-card {
        text-align: left;
      }
      .ref-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: 1rem;
      }
      .reference {
        font-family: monospace;
        font-size: 1.25rem;
        font-weight: 700;
      }
      .grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 0.75rem 1rem;
        margin-bottom: 1.25rem;
      }
      .label {
        color: rgba(0, 0, 0, 0.55);
        font-size: 0.8rem;
      }
      .seats-table {
        width: 100%;
        border-collapse: collapse;
        margin-bottom: 1rem;
      }
      .seats-table th {
        text-align: left;
        color: rgba(0, 0, 0, 0.55);
        font-weight: 500;
        border-bottom: 1px solid rgba(0, 0, 0, 0.12);
        padding-bottom: 0.4rem;
      }
      .seats-table td {
        padding: 0.35rem 0;
      }
      .total {
        display: flex;
        justify-content: space-between;
        padding-top: 0.75rem;
        border-top: 1px solid rgba(0, 0, 0, 0.12);
        font-size: 1.1rem;
        margin-bottom: 1.5rem;
      }
      .actions {
        display: flex;
        gap: 0.75rem;
        flex-wrap: wrap;
      }
    `,
  ],
})
export class BookingConfirmationComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly bookingsService = inject(BookingsService);

  protected readonly booking = signal<Booking | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    const reference = this.route.snapshot.paramMap.get('reference') ?? '';
    this.bookingsService.findByReference(reference).subscribe({
      next: (booking) => {
        this.booking.set(booking);
        this.loading.set(false);
      },
      error: (err: ApiError) => {
        this.error.set(err.message || 'Booking not found.');
        this.loading.set(false);
      },
    });
  }
}
