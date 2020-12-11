import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';

@Component({
  selector: 'app-load-test',
  templateUrl: './load-test.component.html',
  styleUrls: ['./load-test.component.scss']
})
export class LoadTestComponent implements OnInit {
  loadtestresults: LoadTestResult;

  constructor(public http: HttpClient, @Inject('BASE_URL') public baseUrl: string) { }

  ngOnInit() {
    this.http.get<LoadTestResult>(this.baseUrl + 'performance/loadtestsummary').subscribe(result => {
      this.loadtestresults = result;
    }, error => console.error(error));
  }

}

class LoadTestChecks {
  fails: number;
  passes: number;
}
class LoadTestIterations {
  count: number;
  rate: number;
}
class LoadTestIterationDurations {
  avg: number;
  max: number;
  med: number;
  min: number;
}
class LoadTestMetrics {
  checks: LoadTestChecks;
  iterations: LoadTestIterations;
  iteration_duration: LoadTestIterationDurations;
}
class LoadTestResult {
  metrics: LoadTestMetrics;
}
