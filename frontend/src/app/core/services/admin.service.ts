import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Booking, BookingStatus } from '../models/booking.model';
import { CoachUtilization, JourneyStats, OccupancyReport, RevenueReport, SegmentLegUtilization } from '../models/admin.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly api = inject(ApiService);

  occupancy(journeyId: string): Observable<OccupancyReport> {
    return this.api.get<OccupancyReport>('admin/occupancy', { journeyId });
  }

  segmentUtilization(journeyId: string): Observable<SegmentLegUtilization[]> {
    return this.api.get<SegmentLegUtilization[]>('admin/segment-utilization', { journeyId });
  }

  coachUtilization(journeyId: string): Observable<CoachUtilization[]> {
    return this.api.get<CoachUtilization[]>('admin/coach-utilization', { journeyId });
  }

  revenue(from?: string, to?: string, trainId?: string): Observable<RevenueReport> {
    return this.api.get<RevenueReport>('admin/revenue', { from, to, trainId });
  }

  journeyStats(journeyId: string): Observable<JourneyStats> {
    return this.api.get<JourneyStats>('admin/journeys/stats', { journeyId });
  }

  bookings(journeyId?: string, status?: BookingStatus): Observable<Booking[]> {
    return this.api.get<Booking[]>('admin/bookings', { journeyId, status });
  }
}
