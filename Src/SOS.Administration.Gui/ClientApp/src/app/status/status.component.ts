import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { format, parseISO, formatDistanceStrict, formatDuration, intervalToDuration, formatDistance, formatDistanceToNow } from 'date-fns'


function dateFormatter(params) {
  if (params.value) {
    return format(parseISO(params.value), 'yyyy-MM-dd HH:mm:ss');
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
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl
  }

  ngOnInit() {
    this.statuses = [];
    this.http.get<ProcessInfo>(this.baseUrl + 'statusinfo/process').subscribe(result => {
      this.processInfo = result;          
    }, error => console.error(error));
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
}
class Provider {
  dataProviderId: number;
  dataProviderIdentifier: string
  processCount: number;
  processStart: Date;
  processEnd: Date;
  rocessStatus: string;
  harvestCount: number;
  harvestStart: Date;
  harvestEnd: Date;
  harvestStatus: string;
  latestIncrementalCount: number;
  latestIncrementalStart: Date;
  latestIncrementalEnd: Date;
  latestIncrementalStatus: string;
}
class ProcessInfo {
  id: string;
  count: string;
  start: Date;
  end: Date;  
  status: string;
  providersInfo: Provider[];
}
class HarvestInfo {
  id: string;
  count: string;
  start: Date;
  end: Date;
  dataLastModified: Date;
  status: string;
}
