import { Component, OnInit, Inject, Input } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ColumnMode } from '@swimlane/ngx-datatable';

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
    this.updateGrid({ offset: 0, pageSize: this.pageSize }, { columnName: 'datasetID', direction: 'asc' });
  }
  private _dataSetId: string = "0";
  @Input()
  get instance(): string { return this._instance }
  set instance(id: string) {
    this._instance = id;
    this.updateGrid({ offset: 0, pageSize: this.pageSize }, { columnName: 'datasetID', direction: 'asc' });
  }
  private _instance: string = "0";
  ColumnMode = ColumnMode;
  page = new Page();
  pageSize: number = 10;
  columnDefs = [{ prop: 'occurrenceID' }, { prop: 'datasetID' }, { name: 'datasetName' }, { name: 'defects' }]
  rowData = [   
  ];
  http: HttpClient;
  baseUrl: string;
  currentSort: Sort = { columnName: 'datasetID', direction: 'asc' };  
  loadingData: boolean = false;
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
    this.page.pageNumber = 0;
    this.page.size = 20;
  }

  ngOnInit() {
    this.setPage({ offset: 0, pageSize: this.pageSize });
  }
  onSort(sortInfo) {
    console.log(sortInfo);
    this.currentSort.columnName = sortInfo.column.prop;
    this.currentSort.direction = sortInfo.newValue;
    this.updateGrid({ offset: 0, pageSize: this.pageSize }, this.currentSort)
  }
  setPage(pageInfo) {    
    this.page.pageNumber = pageInfo.offset;
    this.updateGrid(pageInfo, this.currentSort);
  }
  private updateGrid(pageInfo, sortInfo: Sort) {
    if (this.loadingData || this.dataSetId == "-1") { return; } else { this.loadingData = true; }
    this.rowData = [];
    this.http.get<PagedInvalidObservations>(this.baseUrl + 'invalidobservations/list?dataSetId=' + this._dataSetId + "&instanceId=" + this._instance + "&pageNr=" + pageInfo.offset + "&pageSize=" + pageInfo.pageSize + "&sortField=" + sortInfo.columnName + "&sortOrder=" + sortInfo.direction).subscribe(result => {
      this.rowData = result.observations;
      this.loadingData = false;
      this.page.pageNumber = result.pageNumber;
      this.page.size = result.size;
      this.page.totalElements = result.totalElements;
      this.page.totalPages = result.totalPages;
      }, error => console.error(error));
    }
}

interface InvalidObservation {
  occurrenceID: string;
  datasetID: string;
  datasetName: string;
  defects: string[];
}

interface PagedInvalidObservations {
  observations: InvalidObservation[];
   // The number of elements in the page
  size: number;
  // The total number of elements
  totalElements: number;
  // The total number of pages
  totalPages: number;
  // The current page number
  pageNumber: number;
}
class Sort {
  columnName: string;
  direction: string;
}
class Page {
  // The number of elements in the page
  size: number = 0;
  // The total number of elements
  totalElements: number = 0;
  // The total number of pages
  totalPages: number = 0;
  // The current page number
  pageNumber: number = 0;
}
