import { HttpClient } from '@angular/common/http';
import { Component, Inject, Input, OnInit } from '@angular/core';
import { icon, latLng, marker, tileLayer } from 'leaflet';
import { InvalidLocation } from '../models/invalidlocation';

@Component({
  selector: 'app-invalid-map',
  templateUrl: './invalid-map.component.html',
  styleUrls: ['./invalid-map.component.scss']
})
export class InvalidMapComponent implements OnInit {
  @Input()
  get dataSetId(): string { return this._dataSetId }
  set dataSetId(id: string) {
    this._dataSetId = id;
    this.updateMap();
  }
  private _dataSetId: string = "0";
  @Input()
  get instance(): string { return this._instance }
  set instance(id: string) {
    this._instance = id;
    this.updateMap();
  }
  private _instance: string = "0";
  public options = {
    layers: [
      tileLayer('http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 18, attribution: 'OpenStreetMap' })
    ],
    zoom: 5,
    center: latLng(46.879966, -121.726909)
  };  
  loadingData: boolean = false;
  markerClusterOptions = {};
  markerClusterData = [];
  constructor(public http: HttpClient, @Inject('BASE_URL') public baseUrl: string) {
  }

  ngOnInit() {    
  }

  updateMap() {
    if (this.loadingData || this.dataSetId == -1) { return; } else { this.loadingData = true; }
    this.markerClusterData = [];
    this.loadingData = true;
    this.http.get<InvalidLocation[]>(this.baseUrl + 'invalidobservations?dataSetId=' + this._dataSetId + "&instanceId=" + this._instance).subscribe(result => {
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
}
