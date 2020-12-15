import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { LoadTestResult } from '../models/loadtestmetrics';

@Component({
  selector: 'app-load-test',
  templateUrl: './load-test.component.html',
  styleUrls: ['./load-test.component.scss']
})
export class LoadTestComponent implements OnInit {
  loadtestresults: LoadTestResult[];

  constructor(public http: HttpClient, @Inject('BASE_URL') public baseUrl: string) { }

  ngOnInit() {
    this.http.get<LoadTestResult[]>(this.baseUrl + 'performance/loadtestsummary').subscribe(result => {
      this.loadtestresults = result;
    }, error => console.error(error));
  }

}
