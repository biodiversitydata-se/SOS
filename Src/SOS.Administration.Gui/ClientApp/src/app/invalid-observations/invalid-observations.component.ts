import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tileLayer, latLng, circle, polygon, marker, icon } from 'leaflet';

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
  public markerClusterOptions = {};
  public markerClusterData = [];
  http: HttpClient;
  baseUrl: string;
  loadingData: boolean = false;
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
    this.selectedDataSet.id = 0;
    this.selectedDataSet.name = "All";
    this.updateProviders();
    this.updateMap();
  }
  onChange(event) {        
    this.selectedDataSet = this.dataSets.find(p => p.id == event);
    this.selectedDataSetId = this.selectedDataSet.id;
    this.updateMap();
  }
  updateMap() {
    this.markerClusterData = [];
    this.loadingData = true;
    this.http.get<InvalidLocation[]>(this.baseUrl + 'invalidobservations/' + this.selectedDataSet.id).subscribe(result => {      
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

interface InvalidLocation {
  occurrenceId: string;
  dataSetId: string;
  dataSetName: string;
  lat: number;
  lon: number
}


class DataProvider{
  id: number;
  name: string;
}
