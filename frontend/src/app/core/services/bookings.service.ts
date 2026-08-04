import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Booking, CreateBookingRequest } from '../models/booking.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class BookingsService {
  private readonly api = inject(ApiService);

  create(request: CreateBookingRequest): Observable<Booking> {
    return this.api.post<Booking>('bookings', request);
  }

  findByReference(reference: string): Observable<Booking> {
    return this.api.get<Booking>(`bookings/${reference}`);
  }

  cancel(reference: string): Observable<Booking> {
    return this.api.delete<Booking>(`bookings/${reference}`);
  }
}
