import { CoachType } from './train.model';

export interface SegmentLegUtilization {
  fromStation: string;
  toStation: string;
  bookedSeats: number;
  totalSeats: number;
  utilizationPercent: number;
}

export interface OccupancyReport {
  journeyId: string;
  totalSeats: number;
  overallOccupancyPercent: number;
  legs: SegmentLegUtilization[];
}

export interface CoachUtilization {
  coachId: string;
  coachNumber: string;
  coachType: CoachType;
  totalSeats: number;
  bookedSeats: number;
  availableSeats: number;
  utilizationPercent: number;
}

export interface RevenueByTrain {
  trainId: string;
  trainName: string;
  revenue: number;
  bookingsCount: number;
}

export interface RevenueReport {
  totalRevenue: number;
  bookingsCount: number;
  byTrain: RevenueByTrain[];
}

export interface JourneyStats {
  journeyId: string;
  totalSeats: number;
  overallOccupancyPercent: number;
  confirmedBookings: number;
  cancelledBookings: number;
  segmentUtilization: SegmentLegUtilization[];
  coachUtilization: CoachUtilization[];
}
