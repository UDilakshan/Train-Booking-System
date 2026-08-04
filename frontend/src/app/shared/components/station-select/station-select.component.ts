import { Component, input, output } from '@angular/core';
import { Station } from '../../../core/models/station.model';

/**
 * A native <select>, not mat-select — see journey-search-form.component.ts's usage: Angular
 * Material 22.1.0 has a bug where two or more mat-select instances mounted simultaneously
 * anywhere in the component tree break mat-form-field's content-child detection for ALL of them
 * ("mat-form-field must contain a MatFormFieldControl", dropdown overlay never opens), regardless
 * of which component owns each one. Confirmed via isolated testing: a single mat-select elsewhere
 * in the app works fine; this page always needs two (origin + destination) at once. Native select
 * sidesteps it entirely and needs no Material dependency.
 */
@Component({
  selector: 'app-station-select',
  standalone: true,
  template: `
    <label class="field">
      <span class="field-label">{{ label() }}</span>
      <select [id]="fieldId()" [value]="value()" (change)="onChange($event)" class="native-select">
        <option value="" disabled selected>Select station</option>
        @for (station of stations(); track station.id) {
          <option [value]="station.id" [disabled]="disabledIds().has(station.id)">{{ station.name }}</option>
        }
      </select>
    </label>
  `,
  styles: [
    `
      .field {
        display: flex;
        flex-direction: column;
        gap: 0.3rem;
      }
      .field-label {
        font-size: 0.75rem;
        color: rgba(0, 0, 0, 0.6);
      }
      .native-select {
        height: 3.5rem;
        padding: 0 0.75rem;
        border-radius: 4px;
        border: 1px solid rgba(0, 0, 0, 0.38);
        background: #fff;
        font: inherit;
        font-size: 1rem;
        color: inherit;
      }
      .native-select:focus {
        outline: 2px solid var(--mat-sys-primary, #1a4d8f);
        outline-offset: -1px;
      }
    `,
  ],
})
export class StationSelectComponent {
  readonly label = input.required<string>();
  readonly fieldId = input.required<string>();
  readonly stations = input.required<Station[]>();
  readonly value = input<string>('');
  /** Station ids to grey out — e.g. ones that would produce an invalid (reversed) route given the other field's current value. */
  readonly disabledIds = input<ReadonlySet<string>>(new Set());
  readonly valueChange = output<string>();

  protected onChange(event: Event): void {
    this.valueChange.emit((event.target as HTMLSelectElement).value);
  }
}
