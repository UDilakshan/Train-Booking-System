import { CoachType } from './train.model';

export interface SeatAvailability {
  seatId: string;
  seatNumber: string;
  seatType: string | null;
  coachId: string;
  coachNumber: string;
  coachType: CoachType;
  coachOrder: number;
  isAvailable: boolean;
}
