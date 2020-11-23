import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tileLayer, latLng, circle, polygon, marker, icon } from 'leaflet';
import { ActiveInstanceInfo } from '../models/activeinstanceinfo';
import { DataProvider } from '../models/dataprovider';
import { InvalidLocation } from '../models/invalidlocation';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './invalid-observations.component.html'
})
export class InvalidObservationsComponent {  
  dataSets = [];
  selectedDataSet = new DataProvider();
  selectedDataSetId = 0;
  
  get selectedInstance(): string { return this._selectedInstance }
  set selectedInstance(id: string) {
    this._selectedInstance = id;
  }
  private _selectedInstance: string = "0";
  activeInstance: string = "0";
  http: HttpClient;
  baseUrl: string;
  loadingData: boolean = false;
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
    this.selectedDataSet.id = 0;
    this.selectedDataSet.name = "All";
    this.updateActiveInstance();
    this.updateProviders();
  }
  onChange(event) {        
    this.selectedDataSet = this.dataSets.find(p => p.id == event);
    this.selectedDataSetId = this.selectedDataSet.id;        
  }
  updateActiveInstance() {
    this.http.get<ActiveInstanceInfo>(this.baseUrl + 'statusinfo/activeinstance').subscribe(result => {
      this.selectedInstance = result.activeInstance.toString();
      this.activeInstance = this.selectedInstance;
    }, error => console.error(error));
  }
  updateProviders() {
    this.dataSets = [];
    this.http.get<DataProvider[]>(this.baseUrl + 'dataprovider/').subscribe(result => {
      this.dataSets = result;
      var all = new DataProvider();
      all.id = 0;
      all.name = "All";
      this.dataSets.push(all);
      this.dataSets = this.dataSets.sort((a, b) => {
        return a.id - b.id;
      });
    }, error => console.error(error));
  }
}
