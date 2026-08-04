import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CoachType } from '../models/train.model';
import { ApiService } from './api.service';

export interface FareQuoteResult {
  fare: number;
  distanceKm: number;
  currency: string;
}

@Injectable({ providedIn: 'root' })
export class FareService {
  private readonly api = inject(ApiService);

  quote(journeyId: string, originStationId: string, destinationStationId: string, coachType: CoachType): Observable<FareQuoteResult> {
    return this.api.get<FareQuoteResult>('fare/quote', { journeyId, originStationId, destinationStationId, coachType });
  }
}
