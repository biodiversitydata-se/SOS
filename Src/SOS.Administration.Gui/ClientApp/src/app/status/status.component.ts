import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { format, parseISO, formatDistanceStrict, formatDuration, intervalToDuration, formatDistance, formatDistanceToNow } from 'date-fns'
import { ActiveInstanceInfo } from '../models/activeinstanceinfo';
import { FunctionalTest } from '../models/functionaltest';
import { HangfireJob } from '../models/hangfirejob';
import { ProcessInfo } from '../models/providerinfo';
import { SearchIndexInfo } from '../models/searchindexinfo';
import { TestResults } from '../models/testresults';


function dateFormatter(params) {
  if (params.value) {
    return format(parseISO(params.value), 'yyyy-MM-dd HH:mm:ss');
  }
  else {
    return '';
  }
}

function dateSinceFormatter(params) {
  if (params.value) {
    return 'Running for ' + formatDistanceToNow(parseISO(params.value));
  }
  else {
    return '';
  }
}

@Component({
  selector: 'app-status',
  templateUrl: './status.component.html',
  styleUrls: ['./status.component.scss']
})
export class StatusComponent implements OnInit {
  http: HttpClient;
  baseUrl: string;
  searchindexinfo: SearchIndexInfo;
  statuses = [];
  processInfo: ProcessInfo;

  processColumnDefs = [
    {
      field: 'processStatus', sortable: true, filter: true, resizable: true, cellRenderer: function (params) {
        return params.value == "Success" ? '<svg height="20" width="30"><circle cx="10" cy="10" r="10" fill="green" /></svg><span>' + params.value + '</span>' :
          '<svg height="20" width="30"><circle cx="10" cy="10" r="10" fill="red" /></svg><span>' + params.value + '</span>'
      }
    },
    {
      field: 'harvestStatus', sortable: true, filter: true, resizable: true, cellRenderer: function (params) {
        return params.value == "Success" ? '<svg height="20" width="30"><circle cx="10" cy="10" r="10" fill="green" /></svg><span>' + params.value + '</span>' :
          '<svg height="20" width="30"><circle cx="10" cy="10" r="10" fill="red" /></svg><span>' + params.value + '</span>'
      }
    },    
    { field: 'dataProviderIdentifier', sortable: true, filter: true, resizable: true },
    { field: 'processCount', sortable: true, filter: true, resizable: true },
    { field: 'processStart', sortable: true, filter: true, resizable: true, valueFormatter: dateFormatter },
    { field: 'processEnd', sortable: true, filter: true, resizable: true, valueFormatter: dateFormatter },   
    { field: 'harvestCount', sortable: true, filter: true, resizable: true },
    { field: 'harvestStart', sortable: true, filter: true, resizable: true, valueFormatter: dateFormatter },
    { field: 'harvestEnd', sortable: true, filter: true, resizable: true, valueFormatter: dateFormatter },
    {
      field: 'latestIncrementalStatus', sortable: true, filter: true, resizable: true, cellRenderer: function (params) {
        return params.value == "2" ? '<svg height="20" width="30"><circle cx="10" cy="10" r="10" fill="green" /></svg><span>' + params.value + '</span>' :
          '<svg height="20" width="30"><circle cx="10" cy="10" r="10" fill="red" /></svg><span>' + params.value + '</span>'
      }
    },    
    { field: 'latestIncrementalCount', sortable: true, filter: true, resizable: true },
    { field: 'latestIncrementalStart', sortable: true, filter: true, resizable: true, valueFormatter: dateFormatter },
    { field: 'latestIncrementalEnd', sortable: true, filter: true, resizable: true, valueFormatter: dateFormatter },    
  ];

  processRowData = [
  ];

  searchIndexColumnDefs = [      
    { field: 'node', sortable: true, filter: true, resizable: true },
    { field: 'percentage', sortable: true, filter: true, resizable: true },
    { field: 'diskUsed', sortable: true, filter: true, resizable: true },
    { field: 'diskAvailable', sortable: true, filter: true, resizable: true },
    { field: 'diskTotal', sortable: true, filter: true, resizable: true },
  ];
  processingJobsRowData = [];
  processingJobsColumnDefs = [
    { field: 'invocationData', width: 600, sortable: true, filter: true, resizable: true },
    { field: 'createdAt', sortable: true, filter: true, resizable: true, valueFormatter: dateFormatter },
    { field: 'createdAt', headerName:'Runtime', sortable: true, filter: true, resizable: true, valueFormatter: dateSinceFormatter } 
  ];
  activeInstance: string;
  runningTests: boolean = false;
  completedTests: number = 0;
  failedTests: number = 0;
  totalRuntimeMs: number = 0;
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl
  }

  ngOnInit() {
    this.statuses = [];
    this.http.get<ActiveInstanceInfo>(this.baseUrl + 'statusinfo/activeinstance').subscribe(result => {      
      this.activeInstance = result.activeInstance.toString();
      this.http.get<ProcessInfo>(this.baseUrl + 'statusinfo/process').subscribe(result => {
        this.processInfo = result;
      }, error => console.error(error));
    }, error => console.error(error));
    
    this.http.get<SearchIndexInfo>(this.baseUrl + 'statusinfo/searchindex').subscribe(result => {
      this.searchindexinfo = result;
    }, error => console.error(error));
    this.http.get<HangfireJob[]>(this.baseUrl + 'statusinfo/processing').subscribe(result => {
      this.processingJobsRowData = result;
    }, error => console.error(error));
    this.runTests();
  }
  formatDate(param) {
    return format(parseISO(param), 'yyyy-MM-dd HH:mm:ss')
  }
  formatFrom(param) {
    return formatDistanceToNow(parseISO(param))
  }
  getTimeTaken(start, end) {
    var startDate = parseISO(start);
    var endDate = parseISO(end);
    var duration = intervalToDuration({
      start: startDate,
      end: endDate
    })
    return formatDuration(duration);
  }
  getActiveInfo(providerId: string) {
    if (this.activeInstance == "0" && providerId == "Observation-0") {
      return "(active)";
    }
    if (this.activeInstance == "1" && providerId == "Observation-1") {
      return "(active)";
    }
    return '';
  }
  private runTests() {
    this.runningTests = true;
    this.completedTests = 0;
    this.failedTests = 0;
    this.totalRuntimeMs = 0;
    this.http.get<FunctionalTest[]>(this.baseUrl + 'tests').subscribe(result => {      
      for (let test of result) {
        test.currentStatus = "Unknown";
      }           
      for (let test of result) {        
        this.http.get<TestResults>('tests/' + test.route).subscribe(result => {          
          if (result) {
            this.totalRuntimeMs += result.timeTakenMs;
            for (let message of result.results) {
              if (message.status == "Succeeded") { this.completedTests++; }
              if (message.status == "Failed") { this.failedTests++; }
            }
          }
        }, error => this.completedTests++);
      }
      this.runningTests = false;
    }, error => console.error(error));
  }
}
