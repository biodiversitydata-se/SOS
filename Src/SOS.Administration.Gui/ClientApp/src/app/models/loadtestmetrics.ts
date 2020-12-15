export class LoadTestChecks {
  fails: number;
  passes: number;
}
export class LoadTestIterations {
  count: number;
  rate: number;
}
export class LoadTestIterationDurations {
  avg: number;
  max: number;
  med: number;
  min: number;
}
export class LoadTestMetrics {
  checks: LoadTestChecks;
  iterations: LoadTestIterations;
  iteration_duration: LoadTestIterationDurations;
}

export class LoadTestResult {
  metrics: LoadTestMetrics;
}
