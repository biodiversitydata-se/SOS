import { Component, Inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ColumnMode } from '@swimlane/ngx-datatable';

import * as moment from 'moment';

@Component({
  templateUrl: './application-insights.component.html'
})
export class ApplicationInsightsComponent {
  private _http: HttpClient;
  private _baseUrl: string;

  startDate: Date = null;
  endDate: Date = null;
  top: number;

  loadingData: boolean = false;
  columnMode = ColumnMode;
  columnDefs = [
    { prop: 'date', name: 'Issue date'  },
    { prop: 'endpoint', name: 'Endpoint' },
    { prop: 'method', name: 'Http method' },
    { prop: 'protectedObservations', name: 'Protected observations' },
    { prop: 'requestBody', name: 'Request body' },
    { prop: 'success', name: 'Sucess' },
    { prop: 'httpResponseCode', name: 'Http response code' },
    { prop: 'duration', name: 'Duration' },
    { prop: 'responseCount', name: 'Count' },
    { prop: 'accountId', name: 'APIM account id' },
    { prop: 'userId', name: 'User admin id' }
  ];
  logRows: ILogRow[];

  constructor(private datePipe: DatePipe, http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    moment.locale('sv', {
      longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'YYYY-MM-DD',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY [kl.] HH:mm',
        LLLL: 'dddd D MMMM YYYY [kl.] HH:mm',
        lll: 'D MMM YYYY HH:mm',
        llll: 'ddd D MMM YYYY HH:mm'
      }
    });
    this._http = http;
    this._baseUrl = baseUrl;
    this.top = 100;
  }

  formatDate(date: Date): string {
    return this.datePipe.transform(date, "yyyy-MM-dd HH:mm")
  }

  onSearchFormSubmit(event) {
    this.loadingData = true;

    this._http.get<ILogRow[]>(`${this._baseUrl}applicationInsights/search?from=${this.formatDate(this.startDate)}&to=${this.formatDate(this.endDate)}&top=${this.top}`)
      .subscribe(result => {
        this.logRows = result;
        this.loadingData = false;
      });
  }
}

interface ILogRow {
  accountId: string;
  duration: number;
  date: Date;
  endpoint: string;
  httpResponseCode: number;
  method: string;
  protectedObservations: string;
  requestBody: string;
  responseCount: string;
  success: boolean;
  userId: string;
}
