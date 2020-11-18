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
  private _dataSetId: string;
  columnDefs = [
    { field: 'occurrenceID', sortable: true, filter: true, resizable: true },
    { field: 'datasetID', sortable: true, filter: true, resizable: true },
    { field: 'datasetName', sortable: true, filter: true, resizable: true },
    { field: 'defects', sortable: true, filter: true, resizable: true }
  ];

  rowData = [
    { occurrenceID: '', datasetID: '', datasetName: '', defects:[] },
  
  ];
  http: HttpClient;
  baseUrl: string;
  loadingData: boolean = false;
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
  }

  ngOnInit() {
    this.updateGrid();     
  }

  private updateGrid() {
      this.rowData = [];
    this.loadingData = true;
    this.http.get<InvalidObservation[]>(this.baseUrl + 'invalidobservations/list/' + this._dataSetId).subscribe(result => {
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

