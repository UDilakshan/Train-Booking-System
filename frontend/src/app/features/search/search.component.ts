import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError } from '../../core/models/api-response.model';
import { Journey } from '../../core/models/journey.model';
import { JourneysService } from '../../core/services/journeys.service';
import {
  JourneySearchFormComponent,
  JourneySearchValue,
} from '../../shared/components/journey-search-form/journey-search-form.component';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [RouterLink, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule, MatProgressSpinnerModule, JourneySearchFormComponent],
  template: `
    <div class="container">
      <mat-card class="refine-card">
        <mat-card-content>
          <app-journey-search-form (searched)="onSearch($event)" />
        </mat-card-content>
      </mat-card>

      @if (loading()) {
        <div class="loading"><mat-spinner diameter="32" /></div>
      } @else if (error()) {
        <p class="error">{{ error() }}</p>
      } @else if (journeys().length === 0) {
        <p class="empty">No scheduled trains found for that date. Try another date.</p>
      } @else {
        <div class="results">
          @for (journey of journeys(); track journey.id) {
            <mat-card class="journey-card">
              <mat-card-content class="journey-content">
                <div class="journey-info">
                  <div class="journey-title">
                    <h3>{{ journey.train.name }}</h3>
                    @if (journey.train.isExpress) {
                      <mat-chip-set><mat-chip color="primary" highlighted>Express</mat-chip></mat-chip-set>
                    }
                  </div>
                  <p class="departure"><mat-icon inline>schedule</mat-icon> Departs {{ journey.departureTime }}</p>
                </div>
                <a
                  mat-raised-button
                  color="primary"
                  [routerLink]="['/booking', journey.id]"
                  [queryParams]="{ originStationId: origin, destinationStationId: destination }"
                >
                  Select Seats
                </a>
              </mat-card-content>
            </mat-card>
          }
        </div>
      }
    </div>
  `,
  styles: [
    `
      .container {
        max-width: 900px;
        margin: 0 auto;
        padding: 2rem 1.5rem;
      }
      .refine-card {
        margin-bottom: 1.5rem;
      }
      .loading {
        display: flex;
        justify-content: center;
        padding: 3rem 0;
      }
      .error {
        color: #b3261e;
      }
      .empty {
        color: rgba(0, 0, 0, 0.6);
      }
      .results {
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }
      .journey-content {
        display: flex;
        align-items: center;
        justify-content: space-between;
        flex-wrap: wrap;
        gap: 1rem;
      }
      .journey-title {
        display: flex;
        align-items: center;
        gap: 0.5rem;
      }
      .journey-title h3 {
        margin: 0;
      }
      .departure {
        margin: 0.25rem 0 0;
        color: rgba(0, 0, 0, 0.65);
        display: flex;
        align-items: center;
        gap: 0.25rem;
      }
    `,
  ],
})
export class SearchComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly journeysService = inject(JourneysService);

  protected readonly journeys = signal<Journey[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected origin = '';
  protected destination = '';
  private date = '';

  ngOnInit(): void {
    this.route.queryParamMap.subscribe((params) => {
      this.origin = params.get('originStationId') ?? '';
      this.destination = params.get('destinationStationId') ?? '';
      this.date = params.get('date') ?? '';
      if (this.date) this.search();
    });
  }

  protected onSearch(value: JourneySearchValue): void {
    this.router.navigate(['/search'], { queryParams: value });
  }

  private search(): void {
    this.loading.set(true);
    this.error.set(null);
    this.journeysService.findAll(this.date).subscribe({
      next: (journeys) => {
        this.journeys.set(journeys);
        this.loading.set(false);
      },
      error: (err: ApiError) => {
        this.error.set(err.message || 'Could not load journeys.');
        this.loading.set(false);
      },
    });
  }
}
