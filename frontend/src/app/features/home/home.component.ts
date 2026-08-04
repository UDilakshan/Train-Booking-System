import { Component, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { JourneySearchFormComponent, JourneySearchValue } from '../../shared/components/journey-search-form/journey-search-form.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [MatCardModule, MatIconModule, JourneySearchFormComponent],
  template: `
    <section class="hero">
      <div class="hero-inner">
        <h1>Reserve your seat on the Colombo Fort – Badulla line</h1>
        <p>
          The same seat can be sold to multiple passengers on non-overlapping legs of the journey —
          book only the stretch of track you need.
        </p>
        <mat-card class="search-card">
          <mat-card-content>
            <app-journey-search-form (searched)="onSearch($event)" />
          </mat-card-content>
        </mat-card>
      </div>
    </section>

    <section class="features">
      <mat-card>
        <mat-card-content class="feature">
          <mat-icon color="primary">route</mat-icon>
          <h3>Segment-based booking</h3>
          <p>Book exactly the stations you're travelling between. A seat sold Colombo→Kandy can still be sold Kandy→Badulla on the same trip.</p>
        </mat-card-content>
      </mat-card>
      <mat-card>
        <mat-card-content class="feature">
          <mat-icon color="primary">confirmation_number</mat-icon>
          <h3>Instant confirmation</h3>
          <p>Real-time seat availability and a booking reference issued the moment your ticket is confirmed.</p>
        </mat-card-content>
      </mat-card>
      <mat-card>
        <mat-card-content class="feature">
          <mat-icon color="primary">payments</mat-icon>
          <h3>Fair, transparent fares</h3>
          <p>Distance-based pricing by class, with peak-hour and express-service surcharges applied automatically.</p>
        </mat-card-content>
      </mat-card>
    </section>
  `,
  styles: [
    `
      .hero {
        background: linear-gradient(180deg, rgba(26, 77, 143, 0.06), transparent);
        border-bottom: 1px solid rgba(0, 0, 0, 0.08);
      }
      .hero-inner {
        max-width: 1100px;
        margin: 0 auto;
        padding: 3rem 1.5rem 3.5rem;
      }
      h1 {
        font-size: clamp(1.25rem, 4vw, 2rem);
        line-height: 1.25;
        font-weight: 700;
        white-space: nowrap;
        margin: 0 0 1rem;
      }
      .hero p {
        max-width: 36rem;
        line-height: 1.5;
        color: rgba(0, 0, 0, 0.65);
        margin: 0 0 1.75rem;
      }
      .search-card {
        max-width: 60rem;
      }
      .features {
        max-width: 1100px;
        margin: 2.5rem auto;
        padding: 0 1.5rem;
        display: grid;
        gap: 1rem;
        grid-template-columns: 1fr;
      }
      @media (min-width: 800px) {
        .features {
          grid-template-columns: repeat(3, 1fr);
        }
      }
      .feature {
        display: flex;
        flex-direction: column;
        gap: 0.4rem;
      }
      .feature h3 {
        margin: 0.25rem 0 0;
      }
      .feature p {
        margin: 0;
        color: rgba(0, 0, 0, 0.65);
        font-size: 0.9rem;
      }
    `,
  ],
})
export class HomeComponent {
  private readonly router = inject(Router);

  onSearch(value: JourneySearchValue): void {
    this.router.navigate(['/search'], { queryParams: value });
  }
}
