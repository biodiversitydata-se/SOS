import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ColumnMode } from '@swimlane/ngx-datatable';

@Component({
  templateUrl: './application-insights.component.html'
})
export class ApplicationInsightsComponent {
  private _http: HttpClient;
  private _baseUrl: string;

  startDate: string;
  endDate: string;

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
    { prop: 'accountId', name: 'APIM acouunt id' },
    { prop: 'userId', name: 'User admin id' }
  ];
  logRows: ILogRow[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this._http = http;
    this._baseUrl = baseUrl;
  }

  onSearchFormSubmit(event) {
    this.loadingData = true;

    this._http.get<ILogRow[]>(`${this._baseUrl}applicationInsights/search?from=${this.startDate}&to=${this.endDate}`)
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
