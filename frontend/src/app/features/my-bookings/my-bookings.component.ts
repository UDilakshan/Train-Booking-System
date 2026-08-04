import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { ApiError } from '../../core/models/api-response.model';
import { Booking } from '../../core/models/booking.model';
import { BookingsService } from '../../core/services/bookings.service';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DecimalPipe,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="container">
      <h1>My Bookings</h1>
      <p class="subtitle">Look up a booking with the reference number from your confirmation.</p>

      <form [formGroup]="form" class="lookup-form" (ngSubmit)="lookup()">
        <mat-form-field appearance="outline">
          <mat-label>Booking reference</mat-label>
          <input matInput formControlName="reference" id="reference" placeholder="RWY-XXXXXXXX" maxlength="12" (input)="onReferenceInput($event)" />
          @if (form.controls.reference.hasError('required') && form.controls.reference.touched) {
            <mat-error>Booking reference is required</mat-error>
          } @else if (form.controls.reference.hasError('pattern')) {
            <mat-error>Format should be RWY-XXXXXXXX</mat-error>
          }
        </mat-form-field>
        <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || loading()">Look Up</button>
      </form>

      @if (loading()) {
        <div class="loading"><mat-spinner diameter="32" /></div>
      } @else if (error()) {
        <p class="error">{{ error() }}</p>
      } @else if (booking(); as booking) {
        <mat-card class="ticket-card">
          <mat-card-content>
            <div class="ref-row">
              <span class="reference">{{ booking.bookingReference }}</span>
              <mat-chip-set><mat-chip [color]="booking.status === 'Confirmed' ? 'primary' : 'warn'" highlighted>{{ booking.status }}</mat-chip></mat-chip-set>
            </div>
            <p>{{ booking.passengerName }} &middot; {{ booking.originStationName }} &rarr; {{ booking.destinationStationName }}</p>
            <p class="muted">{{ booking.trainName }} &middot; {{ booking.journeyDate }} &middot; {{ booking.departureTime }}</p>

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
              <span>Total</span>
              <strong>LKR {{ booking.totalFare | number: '1.2-2' }}</strong>
            </div>

            @if (booking.status === 'Confirmed') {
              <button mat-stroked-button color="warn" (click)="cancel(booking.bookingReference)" [disabled]="cancelling()">
                @if (cancelling()) {
                  Cancelling...
                } @else {
                  Cancel Booking
                }
              </button>
            }
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
        padding: 2rem 1.5rem;
      }
      .subtitle {
        color: rgba(0, 0, 0, 0.6);
        margin-top: 0;
      }
      .lookup-form {
        display: flex;
        gap: 0.75rem;
        align-items: start;
        margin-bottom: 1.5rem;
      }
      .lookup-form mat-form-field {
        flex: 1 1 auto;
      }
      .lookup-form button {
        height: 3.5rem;
      }
      .loading {
        display: flex;
        justify-content: center;
        padding: 2rem 0;
      }
      .error {
        color: #b3261e;
      }
      .ref-row {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: 0.5rem;
      }
      .reference {
        font-family: monospace;
        font-weight: 700;
        font-size: 1.1rem;
      }
      .muted {
        color: rgba(0, 0, 0, 0.55);
        font-size: 0.85rem;
      }
      .seats-table {
        width: 100%;
        border-collapse: collapse;
        margin: 1rem 0;
      }
      .seats-table th {
        text-align: left;
        color: rgba(0, 0, 0, 0.55);
        font-weight: 500;
        border-bottom: 1px solid rgba(0, 0, 0, 0.12);
        padding-bottom: 0.35rem;
      }
      .total {
        display: flex;
        justify-content: space-between;
        padding: 0.5rem 0 1rem;
        border-top: 1px solid rgba(0, 0, 0, 0.12);
      }
    `,
  ],
})
export class MyBookingsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly bookingsService = inject(BookingsService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly booking = signal<Booking | null>(null);
  protected readonly loading = signal(false);
  protected readonly cancelling = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    reference: ['', [Validators.required, Validators.pattern(/^RWY-[A-Za-z0-9]{8}$/)]],
  });

  ngOnInit(): void {
    const reference = this.route.snapshot.queryParamMap.get('reference');
    if (reference) {
      this.form.patchValue({ reference });
      this.lookup();
    }
  }

  /** Booking references are always uppercase (see BookingReferenceGenerator) — uppercase as-typed so a lowercase paste still validates. */
  protected onReferenceInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const upper = input.value.toUpperCase();
    if (upper !== input.value) {
      input.value = upper;
    }
    this.form.controls.reference.setValue(upper);
  }

  protected lookup(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set(null);
    this.booking.set(null);
    this.bookingsService.findByReference(this.form.getRawValue().reference.trim()).subscribe({
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

  protected cancel(reference: string): void {
    if (!confirm('Cancel this booking? This cannot be undone.')) return;
    this.cancelling.set(true);
    this.bookingsService.cancel(reference).subscribe({
      next: (booking) => {
        this.booking.set(booking);
        this.cancelling.set(false);
        this.snackBar.open('Booking cancelled.', 'Dismiss', { duration: 4000 });
      },
      error: (err: ApiError) => {
        this.cancelling.set(false);
        this.snackBar.open(err.message || 'Could not cancel this booking.', 'Dismiss', { duration: 6000 });
      },
    });
  }
}
