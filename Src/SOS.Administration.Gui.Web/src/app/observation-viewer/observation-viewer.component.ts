import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, Inject, Input, OnInit } from '@angular/core';
import { circle, icon, latLng, Layer, marker, tileLayer } from 'leaflet';
import { InvalidLocation } from '../models/invalidlocation';
import { PagedObservations } from '../models/observation';
import { RealObservation } from '../models/realobservation';

@Component({
  selector: 'app-observation-viewer',
  templateUrl: './observation-viewer.component.html',
  styleUrls: ['./observation-viewer.component.scss']
})
export class ObservationViewerComponent implements OnInit {
  public query: string;
  markers: Layer[] = [];
  public options = {
    layers: [
      tileLayer('http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 18, attribution: 'OpenStreetMap' })
    ],  
    zoom: 5,
    center: latLng(59.819541868159256, 17.73797190347293)
  };  
  loadingData: boolean = false;
  markerClusterOptions = {};
  markerClusterData = [];
  constructor(public http: HttpClient, @Inject('BASE_URL') public baseUrl: string) {
  }

  ngOnInit() {    
  }

  updateMap() {
    this.markerClusterData = [];
    this.loadingData = true;
    this.http.post<PagedObservations>(this.baseUrl + 'observations', JSON.parse(this.query)).subscribe(result => {
      this.markers = result.records.map(function (val, index) {
        var description = '<span>' + val.occurrenceId + '</span><br/><span>diffusionRadius:' + val.diffusionRadius + '</span><br/><span>DatasetId:' + val.dataSetId + '</span>'
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
      var circles = result.records.map(function (val, index) {
        var m = circle([val.lat, val.lon], {
          color: 'red',
          fillColor: '#f03',
          fillOpacity: 0.3,
          radius: val.diffusionRadius
        });
        return m;
      });
      result.records.forEach((val) => {
        this.http.get<RealObservation>(this.baseUrl + 'observations/real/' + val.occurrenceId, {
          headers: new HttpHeaders({
            "Content-Type": "application/json"
          })
        }).subscribe(innerResult => {
          console.log(innerResult);
          var description = '<span>' + val.occurrenceId + '</span>';

          var m = marker([innerResult.lat, innerResult.lon], {
            icon: icon({
              iconSize: [25, 41],
              iconAnchor: [13, 41],
              iconUrl: 'assets/marker-icon-realpos.png',
              shadowUrl: 'assets/marker-shadow.png'
            })
          }).bindPopup(description);;
          this.markers.push(m);
        })
      });
      this.markers = this.markers.concat(circles);
      this.loadingData = false;
    }, error => console.error(error));
  }
}
