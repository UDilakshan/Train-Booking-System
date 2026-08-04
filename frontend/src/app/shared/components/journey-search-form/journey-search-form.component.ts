import { Component, OnInit, computed, inject, output, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Station } from '../../../core/models/station.model';
import { StationsService } from '../../../core/services/stations.service';
import { StationSelectComponent } from '../station-select/station-select.component';

export interface JourneySearchValue {
  originStationId: string;
  destinationStationId: string;
  date: string;
}

@Component({
  selector: 'app-journey-search-form',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, StationSelectComponent],
  template: `
    <form class="search-form" (submit)="$event.preventDefault(); submit()">
      <app-station-select
        label="From"
        fieldId="origin"
        [stations]="stations()"
        [value]="originId()"
        [disabledIds]="originDisabledIds()"
        (valueChange)="setOrigin($event)"
      />

      <app-station-select
        label="To"
        fieldId="destination"
        [stations]="stations()"
        [value]="destinationId()"
        [disabledIds]="destinationDisabledIds()"
        (valueChange)="setDestination($event)"
      />

      <!--
        Plain native input, not mat-form-field/matInput — see StationSelectComponent's doc comment.
        The Angular Material 22.1.0 bug turned out to affect ANY mat-form-field control (not just
        mat-select) once it's used inside a reusable child component rather than directly in a
        route component, so this whole form stays Material-free for its inputs (Button/Icon are
        unaffected — the bug is specific to mat-form-field's content-child detection).
      -->
      <label class="field">
        <span class="field-label">Journey date</span>
        <input
          type="date"
          [value]="date()"
          (change)="date.set($any($event.target).value)"
          [min]="todayIso"
          id="journeyDate"
          class="native-input"
        />
      </label>

      <button mat-raised-button color="primary" type="submit" [disabled]="!canSubmit()">
        <mat-icon>search</mat-icon>
        Search Trains
      </button>
    </form>

    @if (originId() && destinationId() === '' && stations().length > 0) {
      <p class="hint">Only stations after {{ originStation()?.name }} towards Badulla can be a destination.</p>
    }
  `,
  styles: [
    `
      .search-form {
        display: grid;
        grid-template-columns: 1fr;
        gap: 0.75rem;
        align-items: start;
      }
      @media (min-width: 800px) {
        .search-form {
          grid-template-columns: 1fr 1fr 1fr auto;
          align-items: center;
        }
      }
      button[mat-raised-button] {
        height: 3.5rem;
      }
      .field {
        display: flex;
        flex-direction: column;
        gap: 0.3rem;
      }
      .field-label {
        font-size: 0.75rem;
        color: rgba(0, 0, 0, 0.6);
      }
      .native-input {
        height: 3.5rem;
        padding: 0 0.75rem;
        border-radius: 4px;
        border: 1px solid rgba(0, 0, 0, 0.38);
        background: #fff;
        font: inherit;
        font-size: 1rem;
        color: inherit;
      }
      .native-input:focus {
        outline: 2px solid var(--mat-sys-primary, #1a4d8f);
        outline-offset: -1px;
      }
      .hint {
        margin: 0.5rem 0 0;
        font-size: 0.8rem;
        color: rgba(0, 0, 0, 0.55);
      }
    `,
  ],
})
export class JourneySearchFormComponent implements OnInit {
  private readonly stationsService = inject(StationsService);

  readonly searched = output<JourneySearchValue>();
  protected readonly stations = signal<Station[]>([]);

  protected readonly todayIso = toIsoDate(new Date());

  protected readonly originId = signal('');
  protected readonly destinationId = signal('');
  protected readonly date = signal(this.todayIso);

  protected readonly originStation = computed(() => this.stations().find((s) => s.id === this.originId()));
  protected readonly destinationStation = computed(() => this.stations().find((s) => s.id === this.destinationId()));

  /**
   * Colombo Fort → Badulla is a single, one-directional route (station Order increases along it),
   * so "From" must sit strictly before "To". Rather than let an invalid pair through to submit and
   * fail with a raw backend error several screens later, disable the stations that would produce
   * one — origin options at or after the current destination, and vice versa — right in the picker.
   * Computed as signals (not a function passed across component boundaries) so the disabled set
   * recomputes reliably whenever either selection changes.
   */
  protected readonly originDisabledIds = computed(() => {
    const destination = this.destinationStation();
    if (!destination) return new Set<string>();
    return new Set(this.stations().filter((s) => s.order >= destination.order).map((s) => s.id));
  });

  protected readonly destinationDisabledIds = computed(() => {
    const origin = this.originStation();
    if (!origin) return new Set<string>();
    return new Set(this.stations().filter((s) => s.order <= origin.order).map((s) => s.id));
  });

  protected readonly canSubmit = computed(() => !!this.originId() && !!this.destinationId() && !!this.date());

  ngOnInit(): void {
    this.stationsService.findAll().subscribe((stations) => this.stations.set(stations));
  }

  protected setOrigin(stationId: string): void {
    this.originId.set(stationId);
    const origin = this.stations().find((s) => s.id === stationId);
    const destination = this.destinationStation();
    if (origin && destination && destination.order <= origin.order) {
      this.destinationId.set('');
    }
  }

  protected setDestination(stationId: string): void {
    this.destinationId.set(stationId);
    const destination = this.stations().find((s) => s.id === stationId);
    const origin = this.originStation();
    if (origin && destination && destination.order <= origin.order) {
      this.originId.set('');
    }
  }

  submit(): void {
    if (!this.canSubmit()) return;
    this.searched.emit({ originStationId: this.originId(), destinationStationId: this.destinationId(), date: this.date() });
  }
}

function toIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}
