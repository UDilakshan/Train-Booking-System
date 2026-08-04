import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Station } from '../models/station.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class StationsService {
  private readonly api = inject(ApiService);

  findAll(): Observable<Station[]> {
    return this.api.get<Station[]>('stations');
  }

  findOne(id: string): Observable<Station> {
    return this.api.get<Station>(`stations/${id}`);
  }
}
