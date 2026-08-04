export type CoachType = 'FirstClass' | 'SecondClass' | 'ThirdClass' | 'Observation';

export interface Coach {
  id: string;
  trainId: string;
  coachNumber: string;
  coachType: CoachType;
  order: number;
  seats?: Seat[];
}

export interface Seat {
  id: string;
  coachId: string;
  seatNumber: string;
  seatType: string | null;
}

export interface Train {
  id: string;
  code: string;
  name: string;
  description: string | null;
  isExpress: boolean;
  isActive: boolean;
  coaches?: Coach[];
}
