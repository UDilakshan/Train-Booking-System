import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ApiError } from '../../core/models/api-response.model';
import { SeatAvailability } from '../../core/models/availability.model';
import { Journey } from '../../core/models/journey.model';
import { Station } from '../../core/models/station.model';
import { AvailabilityService } from '../../core/services/availability.service';
import { BookingsService } from '../../core/services/bookings.service';
import { FareService } from '../../core/services/fare.service';
import { JourneysService } from '../../core/services/journeys.service';
import { StationsService } from '../../core/services/stations.service';
import { SeatMapComponent } from '../../shared/components/seat-map/seat-map.component';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [ReactiveFormsModule, DecimalPipe, MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, SeatMapComponent],
  template: `
    @if (loading()) {
      <div class="loading"><mat-spinner diameter="32" /></div>
    } @else if (loadError()) {
      <p class="error container">{{ loadError() }}</p>
    } @else {
      <div class="page">
        <div class="seat-map-column">
          <h1>{{ journey()?.train?.name }}</h1>
          <p class="route">{{ origin()?.name }} &rarr; {{ destination()?.name }} &middot; {{ journey()?.departureTime }}</p>
          <app-seat-map [seats]="seats()" [selectedSeatIds]="selectedSeatIds()" (seatToggle)="toggleSeat($event)" />
        </div>

        <aside class="summary-column">
          <mat-card class="summary-card">
            <mat-card-header>
              <mat-card-title>Booking Summary</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <p class="route">{{ origin()?.name }} &rarr; {{ destination()?.name }}</p>

              @if (selectedSeats().length === 0) {
                <p class="hint">Select at least one seat to see the fare.</p>
              } @else {
                <table class="fare-table">
                  <thead>
                    <tr>
                      <th>Coach</th>
                      <th>Seat</th>
                      <th>Fare</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (seat of selectedSeats(); track seat.seatId) {
                      <tr>
                        <td>{{ seat.coachNumber }}</td>
                        <td>{{ seat.seatNumber }}</td>
                        <td>{{ fareFor(seat) | number: '1.2-2' }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
                <div class="total">
                  <span>Total</span>
                  <strong>LKR {{ totalFare() | number: '1.2-2' }}</strong>
                </div>
              }

              <form [formGroup]="form" class="passenger-form">
                <mat-form-field appearance="outline">
                  <mat-label>Passenger name</mat-label>
                  <input matInput formControlName="passengerName" id="passengerName" maxlength="120" />
                  @if (form.controls.passengerName.hasError('required') && form.controls.passengerName.touched) {
                    <mat-error>Passenger name is required</mat-error>
                  } @else if (form.controls.passengerName.hasError('pattern')) {
                    <mat-error>Name can only contain letters, spaces, apostrophes and hyphens</mat-error>
                  }
                </mat-form-field>
                <mat-form-field appearance="outline">
                  <mat-label>Contact number</mat-label>
                  <input
                    matInput
                    formControlName="passengerContact"
                    id="passengerContact"
                    inputmode="numeric"
                    maxlength="10"
                    placeholder="0712345678"
                    (input)="onContactInput($event)"
                    (keydown)="blockNonDigitKeys($event)"
                    (paste)="onContactPaste($event)"
                  />
                  @if (form.controls.passengerContact.hasError('required') && form.controls.passengerContact.touched) {
                    <mat-error>Contact number is required</mat-error>
                  } @else if (form.controls.passengerContact.hasError('pattern')) {
                    <mat-error>Contact number must be exactly 10 digits</mat-error>
                  }
                </mat-form-field>
                <button
                  mat-raised-button
                  color="primary"
                  type="button"
                  [disabled]="selectedSeats().length === 0 || form.invalid || submitting()"
                  (click)="confirmBooking()"
                >
                  @if (submitting()) {
                    Confirming...
                  } @else {
                    Confirm Booking
                  }
                </button>
              </form>
            </mat-card-content>
          </mat-card>
        </aside>
      </div>
    }
  `,
  styles: [
    `
      .loading {
        display: flex;
        justify-content: center;
        padding: 3rem 0;
      }
      .container {
        max-width: 900px;
        margin: 0 auto;
        padding: 2rem 1.5rem;
      }
      .page {
        max-width: 1200px;
        margin: 0 auto;
        padding: 2rem 1.5rem;
        display: grid;
        gap: 1.5rem;
        grid-template-columns: 1fr;
      }
      @media (min-width: 960px) {
        .page {
          grid-template-columns: 2fr 1fr;
        }
      }
      h1 {
        margin: 0 0 0.25rem;
      }
      .route {
        color: rgba(0, 0, 0, 0.65);
        margin: 0 0 1.25rem;
      }
      .summary-card {
        position: sticky;
        top: 5rem;
      }
      .hint {
        color: rgba(0, 0, 0, 0.55);
        font-size: 0.9rem;
      }
      .fare-table {
        width: 100%;
        border-collapse: collapse;
        font-size: 0.85rem;
        margin-bottom: 0.75rem;
      }
      .fare-table th {
        text-align: left;
        color: rgba(0, 0, 0, 0.55);
        font-weight: 500;
        padding-bottom: 0.25rem;
      }
      .fare-table td {
        padding: 0.2rem 0;
      }
      .total {
        display: flex;
        justify-content: space-between;
        padding-top: 0.5rem;
        border-top: 1px solid rgba(0, 0, 0, 0.12);
        margin-bottom: 1rem;
      }
      .passenger-form {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      .passenger-form button {
        margin-top: 0.5rem;
      }
      .error {
        color: #b3261e;
      }
    `,
  ],
})
export class BookingComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);
  private readonly journeysService = inject(JourneysService);
  private readonly stationsService = inject(StationsService);
  private readonly availabilityService = inject(AvailabilityService);
  private readonly fareService = inject(FareService);
  private readonly bookingsService = inject(BookingsService);

  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly submitting = signal(false);

  protected readonly journey = signal<Journey | null>(null);
  protected readonly origin = signal<Station | null>(null);
  protected readonly destination = signal<Station | null>(null);
  protected readonly seats = signal<SeatAvailability[]>([]);
  protected readonly selectedSeatIds = signal<ReadonlySet<string>>(new Set());
  private readonly fareByCoachType = signal<Record<string, number>>({});

  protected readonly selectedSeats = computed(() => this.seats().filter((s) => this.selectedSeatIds().has(s.seatId)));
  protected readonly totalFare = computed(() => this.selectedSeats().reduce((sum, s) => sum + this.fareFor(s), 0));

  protected readonly form = this.fb.nonNullable.group({
    passengerName: ['', [Validators.required, Validators.maxLength(120), Validators.pattern(/^[A-Za-z][A-Za-z .'-]*$/)]],
    passengerContact: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]],
  });

  private journeyId = '';
  private originStationId = '';
  private destinationStationId = '';

  ngOnInit(): void {
    this.journeyId = this.route.snapshot.paramMap.get('journeyId') ?? '';
    this.originStationId = this.route.snapshot.queryParamMap.get('originStationId') ?? '';
    this.destinationStationId = this.route.snapshot.queryParamMap.get('destinationStationId') ?? '';

    if (!this.journeyId || !this.originStationId || !this.destinationStationId) {
      this.loadError.set('Missing journey or station selection. Please search again.');
      this.loading.set(false);
      return;
    }

    forkJoin({
      journey: this.journeysService.findOne(this.journeyId),
      origin: this.stationsService.findOne(this.originStationId),
      destination: this.stationsService.findOne(this.destinationStationId),
      seats: this.availabilityService.get(this.journeyId, this.originStationId, this.destinationStationId),
    }).subscribe({
      next: ({ journey, origin, destination, seats }) => {
        this.journey.set(journey);
        this.origin.set(origin);
        this.destination.set(destination);
        this.seats.set(seats);
        this.loading.set(false);
      },
      error: (err: ApiError) => {
        this.loadError.set(err.message || 'Could not load this journey.');
        this.loading.set(false);
      },
    });
  }

  /** Strips anything but digits as the user types, so letters/symbols never make it into the field. */
  protected onContactInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const digitsOnly = input.value.replace(/\D/g, '').slice(0, 10);
    if (digitsOnly !== input.value) {
      input.value = digitsOnly;
    }
    this.form.controls.passengerContact.setValue(digitsOnly);
  }

  /** Belt-and-braces: blocks digit-incapable keys before they even render, for browsers/IMEs where (input) filtering lags a frame. */
  protected blockNonDigitKeys(event: KeyboardEvent): void {
    const allowedControlKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];
    if (allowedControlKeys.includes(event.key) || event.ctrlKey || event.metaKey) return;
    if (!/^[0-9]$/.test(event.key)) {
      event.preventDefault();
    }
  }

  protected onContactPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const pasted = event.clipboardData?.getData('text') ?? '';
    const digitsOnly = pasted.replace(/\D/g, '').slice(0, 10);
    this.form.controls.passengerContact.setValue(digitsOnly);
  }

  protected toggleSeat(seat: SeatAvailability): void {
    const next = new Set(this.selectedSeatIds());
    if (next.has(seat.seatId)) {
      next.delete(seat.seatId);
    } else {
      next.add(seat.seatId);
      this.ensureFareLoaded(seat.coachType);
    }
    this.selectedSeatIds.set(next);
  }

  protected fareFor(seat: SeatAvailability): number {
    return this.fareByCoachType()[seat.coachType] ?? 0;
  }

  private ensureFareLoaded(coachType: string): void {
    if (this.fareByCoachType()[coachType] !== undefined) return;
    this.fareService.quote(this.journeyId, this.originStationId, this.destinationStationId, coachType as never).subscribe((result) => {
      this.fareByCoachType.update((map) => ({ ...map, [coachType]: result.fare }));
    });
  }

  protected confirmBooking(): void {
    if (this.form.invalid || this.selectedSeats().length === 0) return;
    this.submitting.set(true);
    const { passengerName, passengerContact } = this.form.getRawValue();

    this.bookingsService
      .create({
        journeyId: this.journeyId,
        originStationId: this.originStationId,
        destinationStationId: this.destinationStationId,
        passengerName: passengerName.trim(),
        passengerContact,
        seatIds: this.selectedSeats().map((s) => s.seatId),
      })
      .subscribe({
        next: (booking) => {
          this.router.navigate(['/booking/confirmation', booking.bookingReference]);
        },
        error: (err: ApiError) => {
          this.submitting.set(false);
          this.snackBar.open(err.message || 'Booking failed. Please try again.', 'Dismiss', { duration: 6000 });
          if (err.code === 'SEGMENT_OVERLAP') {
            // Someone else booked this seat first — refresh availability so the map reflects reality.
            this.availabilityService.get(this.journeyId, this.originStationId, this.destinationStationId).subscribe((seats) => {
              this.seats.set(seats);
              this.selectedSeatIds.set(new Set());
            });
          }
        },
      });
  }
}
