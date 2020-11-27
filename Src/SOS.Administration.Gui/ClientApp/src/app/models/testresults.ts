import { TestResult } from "./testresult";

export class TestResults {
  testId: number;
  timeTakenMs: number;
  results: TestResult[];
}
