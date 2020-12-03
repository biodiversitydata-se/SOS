import { Component, OnInit, Inject, Input } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-invalid-grid',
  templateUrl: './invalid-grid.component.html',
  styleUrls: ['./invalid-grid.component.scss']
})
export class InvalidGridComponent implements OnInit {
  @Input()
  get dataSetId(): string { return this._dataSetId }
  set dataSetId(id: string) {
    this._dataSetId = id;
    this.updateGrid();
  }
  private _dataSetId: string = "0";
  @Input()
  get instance(): string { return this._instance }
  set instance(id: string) {
    this._instance = id;
    this.updateGrid();
  }
  private _instance: string = "0";
  columnDefs = [
    { field: 'occurrenceID', sortable: true, filter: true, resizable: true },
    { field: 'datasetID', sortable: true, filter: true, resizable: true },
    { field: 'datasetName', sortable: true, filter: true, resizable: true },
    { field: 'defects', sortable: true, filter: true, resizable: true }
  ];

  rowData = [   
  ];
  http: HttpClient;
  baseUrl: string;
  loadingData: boolean = false;
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
  }

  ngOnInit() {    
  }

  private updateGrid() {
    if (this.loadingData || this.dataSetId == "-1") { return; } else { this.loadingData = true; }
    this.rowData = [];    
    this.http.get<InvalidObservation[]>(this.baseUrl + 'invalidobservations/list?dataSetId=' + this._dataSetId + "&instanceId=" + this._instance).subscribe(result => {
          this.rowData = result.map(function(val, index) {
              return val;
          });
          this.loadingData = false;
      }, error => console.error(error));
    }
}

interface InvalidObservation {
  occurrenceID: string;
  datasetID: string;
  datasetName: string;
  defects: string[];
}

