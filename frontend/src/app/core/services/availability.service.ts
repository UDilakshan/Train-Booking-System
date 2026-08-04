import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SeatAvailability } from '../models/availability.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class AvailabilityService {
  private readonly api = inject(ApiService);

  get(journeyId: string, originStationId: string, destinationStationId: string): Observable<SeatAvailability[]> {
    return this.api.get<SeatAvailability[]>('availability', { journeyId, originStationId, destinationStationId });
  }
}
