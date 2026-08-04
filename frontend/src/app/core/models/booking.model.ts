import { CoachType } from './train.model';

export type BookingStatus = 'Confirmed' | 'Cancelled';

export interface BookingSegment {
  id: string;
  seatId: string;
  seatNumber: string;
  coachId: string;
  coachNumber: string;
  coachType: CoachType;
  originOrder: number;
  destinationOrder: number;
  fare: number;
  status: BookingStatus;
}

export interface Booking {
  id: string;
  bookingReference: string;
  journeyId: string;
  journeyDate: string;
  departureTime: string;
  trainName: string;
  originStationId: string;
  originStationName: string;
  destinationStationId: string;
  destinationStationName: string;
  passengerName: string;
  passengerContact: string;
  totalFare: number;
  status: BookingStatus;
  createdAt: string;
  segments: BookingSegment[];
}

export interface CreateBookingRequest {
  journeyId: string;
  originStationId: string;
  destinationStationId: string;
  passengerName: string;
  passengerContact: string;
  seatIds: string[];
}
