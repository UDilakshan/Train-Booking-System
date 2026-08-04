import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Journey } from '../models/journey.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class JourneysService {
  private readonly api = inject(ApiService);

  findAll(date?: string, trainId?: string): Observable<Journey[]> {
    return this.api.get<Journey[]>('journeys', { date, trainId });
  }

  findOne(id: string): Observable<Journey> {
    return this.api.get<Journey>(`journeys/${id}`);
  }
}
