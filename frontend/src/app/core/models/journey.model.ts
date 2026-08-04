import { Train } from './train.model';

export type JourneyStatus = 'Scheduled' | 'Cancelled' | 'Completed';

export interface Journey {
  id: string;
  trainId: string;
  journeyDate: string;
  departureTime: string;
  status: JourneyStatus;
  train: Train;
}
