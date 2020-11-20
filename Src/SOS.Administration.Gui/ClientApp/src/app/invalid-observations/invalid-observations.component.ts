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
  public options = {
    layers: [
      tileLayer('http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 18, attribution: 'OpenStreetMap' })
    ],
    zoom: 5,
    center: latLng(46.879966, -121.726909)
  };
  get selectedInstance(): string { return this._selectedInstance }
  set selectedInstance(id: string) {
    this._selectedInstance = id;
    this.updateMap();
  }
  private _selectedInstance: string = "0";
  activeInstance: string = "0";
  markerClusterOptions = {};
  markerClusterData = [];
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
    this.updateMap();
  }
  updateActiveInstance() {
    this.http.get<ActiveInstanceInfo>(this.baseUrl + 'statusinfo/activeinstance').subscribe(result => {
      this.selectedInstance = result.activeInstance.toString();
      this.activeInstance = this.selectedInstance;
    }, error => console.error(error));
  }
  updateMap() {
    this.markerClusterData = [];
    this.loadingData = true;
    this.http.get<InvalidLocation[]>(this.baseUrl + 'invalidobservations?dataSetId=' + this.selectedDataSet.id + "&instanceId=" + this.selectedInstance).subscribe(result => {      
      this.markerClusterData = result.map(function (val, index) {
        var description = '<span>Id:' + val.occurrenceId + '</span><br/><span>DatasetName:' + val.dataSetName + '</span><br/><span>DatasetId:' + val.dataSetId + '</span>'
        var m = marker([val.lat, val.lon], {
          icon: icon({
            iconSize: [25, 41],
            iconAnchor: [13, 41],
            iconUrl: 'assets/marker-icon.png',
            shadowUrl: 'assets/marker-shadow.png'
          })
        }).bindPopup(description);        
        return m;
      });
      this.loadingData = false;
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
