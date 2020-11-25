export interface FunctionalTest {
  group: string;
  description: string;
  route: string;
  timeTakenMs: number;
  currentStatus: string;
}
