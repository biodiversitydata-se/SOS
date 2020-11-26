export interface FunctionalTest {
  group: string;
  description: string;
  route: string;
  id: number;
  timeTakenMs: number;
  currentStatus: string;
  errorMessages: string;
}
