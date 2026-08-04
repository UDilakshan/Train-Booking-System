import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';
import { Router } from '@angular/router';
import { Booking } from '../../../core/models/booking.model';
import { JourneyStats, RevenueReport } from '../../../core/models/admin.model';
import { Journey } from '../../../core/models/journey.model';
import { AdminService } from '../../../core/services/admin.service';
import { AuthService } from '../../../core/services/auth.service';
import { JourneysService } from '../../../core/services/journeys.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    DecimalPipe,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatTabsModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="container">
      <div class="header">
        <div>
          <h1>Admin Dashboard</h1>
          <p class="subtitle">Occupancy, revenue and booking oversight.</p>
        </div>
        <button mat-stroked-button (click)="logout()">Log Out</button>
      </div>

      <div class="filters">
        <mat-form-field appearance="outline">
          <mat-label>Journey date</mat-label>
          <input matInput type="date" [(ngModel)]="date" (ngModelChange)="onDateChange()" id="admin-date" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Journey</mat-label>
          <mat-select [(ngModel)]="selectedJourneyId" (ngModelChange)="onJourneyChange()" id="admin-journey">
            @for (journey of journeys(); track journey.id) {
              <mat-option [value]="journey.id">{{ journey.train.name }} &middot; {{ journey.departureTime }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
      </div>

      @if (statsLoading()) {
        <div class="loading"><mat-spinner diameter="32" /></div>
      } @else if (stats(); as stats) {
        <div class="stat-grid">
          <mat-card class="stat-card">
            <mat-card-content>
              <span class="stat-label">Occupancy</span>
              <span class="stat-value">{{ stats.overallOccupancyPercent | number: '1.1-1' }}%</span>
              <span class="stat-sub">Average across all legs</span>
            </mat-card-content>
          </mat-card>
          <mat-card class="stat-card">
            <mat-card-content>
              <span class="stat-label">Total Seats</span>
              <span class="stat-value">{{ stats.totalSeats }}</span>
            </mat-card-content>
          </mat-card>
          <mat-card class="stat-card">
            <mat-card-content>
              <span class="stat-label">Confirmed Bookings</span>
              <span class="stat-value">{{ stats.confirmedBookings }}</span>
            </mat-card-content>
          </mat-card>
          <mat-card class="stat-card">
            <mat-card-content>
              <span class="stat-label">Cancelled Bookings</span>
              <span class="stat-value">{{ stats.cancelledBookings }}</span>
            </mat-card-content>
          </mat-card>
        </div>

        <mat-tab-group (selectedTabChange)="onTabChange($event.index)">
          <mat-tab label="Segment Utilization">
            <table class="data-table">
              <thead><tr><th>From</th><th>To</th><th>Booked / Total</th><th>Utilization</th></tr></thead>
              <tbody>
                @for (leg of stats.segmentUtilization; track leg.fromStation + leg.toStation) {
                  <tr>
                    <td>{{ leg.fromStation }}</td>
                    <td>{{ leg.toStation }}</td>
                    <td>{{ leg.bookedSeats }} / {{ leg.totalSeats }}</td>
                    <td>{{ leg.utilizationPercent | number: '1.1-1' }}%</td>
                  </tr>
                }
              </tbody>
            </table>
          </mat-tab>

          <mat-tab label="Coach Utilization">
            <table class="data-table">
              <thead><tr><th>Coach</th><th>Class</th><th>Booked</th><th>Available</th><th>Utilization</th></tr></thead>
              <tbody>
                @for (coach of stats.coachUtilization; track coach.coachId) {
                  <tr>
                    <td>{{ coach.coachNumber }}</td>
                    <td>{{ coach.coachType }}</td>
                    <td>{{ coach.bookedSeats }} / {{ coach.totalSeats }}</td>
                    <td>{{ coach.availableSeats }}</td>
                    <td>{{ coach.utilizationPercent | number: '1.1-1' }}%</td>
                  </tr>
                }
              </tbody>
            </table>
          </mat-tab>

          <mat-tab label="Bookings">
            @if (bookingsLoading()) {
              <div class="loading"><mat-spinner diameter="24" /></div>
            } @else {
              <table class="data-table">
                <thead><tr><th>Reference</th><th>Passenger</th><th>Route</th><th>Status</th><th>Fare</th></tr></thead>
                <tbody>
                  @for (booking of bookings(); track booking.id) {
                    <tr>
                      <td class="mono">{{ booking.bookingReference }}</td>
                      <td>{{ booking.passengerName }}</td>
                      <td>{{ booking.originStationName }} &rarr; {{ booking.destinationStationName }}</td>
                      <td>{{ booking.status }}</td>
                      <td>LKR {{ booking.totalFare | number: '1.2-2' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            }
          </mat-tab>

          <mat-tab label="Revenue">
            @if (revenueLoading()) {
              <div class="loading"><mat-spinner diameter="24" /></div>
            } @else if (revenue(); as revenue) {
              <div class="revenue-summary">
                <span class="stat-label">Total Revenue (all trains, all time)</span>
                <span class="stat-value">LKR {{ revenue.totalRevenue | number: '1.2-2' }}</span>
                <span class="stat-sub">{{ revenue.bookingsCount }} confirmed bookings</span>
              </div>
              <table class="data-table">
                <thead><tr><th>Train</th><th>Revenue</th><th>Bookings</th></tr></thead>
                <tbody>
                  @for (row of revenue.byTrain; track row.trainId) {
                    <tr>
                      <td>{{ row.trainName }}</td>
                      <td>LKR {{ row.revenue | number: '1.2-2' }}</td>
                      <td>{{ row.bookingsCount }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            }
          </mat-tab>
        </mat-tab-group>
      } @else {
        <p class="hint">Select a journey to view occupancy, bookings, and revenue.</p>
      }
    </div>
  `,
  styles: [
    `
      .container {
        max-width: 1100px;
        margin: 0 auto;
        padding: 2rem 1.5rem;
      }
      .header {
        display: flex;
        justify-content: space-between;
        align-items: start;
        margin-bottom: 1rem;
      }
      h1 {
        margin: 0;
      }
      .subtitle {
        color: rgba(0, 0, 0, 0.6);
        margin: 0.25rem 0 0;
      }
      .filters {
        display: flex;
        gap: 1rem;
        flex-wrap: wrap;
        margin-bottom: 1rem;
      }
      .loading {
        display: flex;
        justify-content: center;
        padding: 2rem 0;
      }
      .hint {
        color: rgba(0, 0, 0, 0.55);
      }
      .stat-grid {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: 1rem;
        margin-bottom: 1.5rem;
      }
      @media (min-width: 800px) {
        .stat-grid {
          grid-template-columns: repeat(4, 1fr);
        }
      }
      .stat-card mat-card-content {
        display: flex;
        flex-direction: column;
      }
      .stat-label {
        color: rgba(0, 0, 0, 0.55);
        font-size: 0.8rem;
      }
      .stat-value {
        font-size: 1.6rem;
        font-weight: 700;
      }
      .stat-sub {
        font-size: 0.75rem;
        color: rgba(0, 0, 0, 0.5);
      }
      .data-table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 1rem;
      }
      .data-table th {
        text-align: left;
        color: rgba(0, 0, 0, 0.55);
        font-weight: 500;
        border-bottom: 1px solid rgba(0, 0, 0, 0.12);
        padding: 0.5rem 0.5rem;
      }
      .data-table td {
        padding: 0.4rem 0.5rem;
        border-bottom: 1px solid rgba(0, 0, 0, 0.06);
      }
      .mono {
        font-family: monospace;
      }
      .revenue-summary {
        display: flex;
        flex-direction: column;
        margin-top: 1rem;
      }
    `,
  ],
})
export class AdminDashboardComponent implements OnInit {
  private readonly journeysService = inject(JourneysService);
  private readonly adminService = inject(AdminService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected date = new Date().toISOString().slice(0, 10);
  protected selectedJourneyId = '';

  protected readonly journeys = signal<Journey[]>([]);
  protected readonly stats = signal<JourneyStats | null>(null);
  protected readonly statsLoading = signal(false);

  protected readonly bookings = signal<Booking[]>([]);
  protected readonly bookingsLoading = signal(false);
  private bookingsLoaded = false;

  protected readonly revenue = signal<RevenueReport | null>(null);
  protected readonly revenueLoading = signal(false);
  private revenueLoaded = false;

  ngOnInit(): void {
    this.loadJourneys();
  }

  protected onDateChange(): void {
    this.selectedJourneyId = '';
    this.stats.set(null);
    this.loadJourneys();
  }

  protected onJourneyChange(): void {
    this.bookingsLoaded = false;
    this.revenueLoaded = false;
    this.bookings.set([]);
    this.revenue.set(null);
    this.loadStats();
  }

  protected onTabChange(index: number): void {
    if (index === 2 && !this.bookingsLoaded) this.loadBookings();
    if (index === 3 && !this.revenueLoaded) this.loadRevenue();
  }

  protected logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  private loadJourneys(): void {
    this.journeysService.findAll(this.date).subscribe((journeys) => {
      this.journeys.set(journeys);
      if (journeys.length > 0) {
        this.selectedJourneyId = journeys[0].id;
        this.loadStats();
      }
    });
  }

  private loadStats(): void {
    if (!this.selectedJourneyId) return;
    this.statsLoading.set(true);
    this.adminService.journeyStats(this.selectedJourneyId).subscribe((stats) => {
      this.stats.set(stats);
      this.statsLoading.set(false);
    });
  }

  private loadBookings(): void {
    this.bookingsLoading.set(true);
    this.adminService.bookings(this.selectedJourneyId).subscribe((bookings) => {
      this.bookings.set(bookings);
      this.bookingsLoading.set(false);
      this.bookingsLoaded = true;
    });
  }

  private loadRevenue(): void {
    this.revenueLoading.set(true);
    this.adminService.revenue().subscribe((revenue) => {
      this.revenue.set(revenue);
      this.revenueLoading.set(false);
      this.revenueLoaded = true;
    });
  }
}
