import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { ChartDataSets, ChartOptions, ChartType } from 'chart.js';
import { Color, BaseChartDirective, Label } from 'ng2-charts';
import * as pluginAnnotations from 'chartjs-plugin-annotation';
import { HttpClient } from '@angular/common/http';
import { FunctionalTest } from '../models/functionaltest';
import { add, formatISO, sub } from 'date-fns';
import  stc from 'string-to-color'
import { PerformanceData } from '../models/performancedata';


@Component({
  selector: 'app-performance-chart',
  templateUrl: './performance-chart.component.html',
  styleUrls: ['./performance-chart.component.scss']
})
export class PerformanceChartComponent implements OnInit {
  public lineChartData: ChartDataSets[] = [ ];  
  public lineChartOptions: (ChartOptions & { annotation: any }) = {
    responsive: true,
    scales: {
      // We use this empty structure as a placeholder for dynamic theming.
      xAxes: [{
        type: 'time'        
      }],
      yAxes: [
        {
          id: 'y-axis-0',
          position: 'left',
          ticks: { callback: function (value, index, values) { return value + 'ms' } },
          scaleLabel: { display: true, labelString: 'Time taken (ms)'}
        }
      ]
    },
    annotation: {
      annotations: [
        {
          type: 'line',
          mode: 'vertical',
          scaleID: 'x-axis-0',
          value: 'March',
          borderColor: 'orange',
          borderWidth: 2,
          label: {
            enabled: true,
            fontColor: 'orange',
            content: 'LineAnno'
          }
        },
      ],
    },
  };
  public lineChartLegend = true;
  public lineChartType: ChartType = 'line';
  public lineChartPlugins = [pluginAnnotations];
  
  public lineChartColors: Color[] = [
    { // grey
      backgroundColor: 'rgba(148,159,177,0.2)',
      borderColor: 'rgba(148,159,177,1)',
      pointBackgroundColor: 'rgba(148,159,177,1)',
      pointBorderColor: '#fff',
      pointHoverBackgroundColor: '#fff',
      pointHoverBorderColor: 'rgba(148,159,177,0.8)'
    },
    { // dark grey
      backgroundColor: 'rgba(77,83,96,0.2)',
      borderColor: 'rgba(77,83,96,1)',
      pointBackgroundColor: 'rgba(77,83,96,1)',
      pointBorderColor: '#fff',
      pointHoverBackgroundColor: '#fff',
      pointHoverBorderColor: 'rgba(77,83,96,1)'
    },
    { // red
      backgroundColor: 'rgba(255,0,0,0.3)',
      borderColor: 'red',
      pointBackgroundColor: 'rgba(148,159,177,1)',
      pointBorderColor: '#fff',
      pointHoverBackgroundColor: '#fff',
      pointHoverBorderColor: 'rgba(148,159,177,0.8)'
    }
  ];
  @ViewChild(BaseChartDirective, { static: true }) chart: BaseChartDirective;  
  filterId = "30m";
  constructor(public http: HttpClient, @Inject('BASE_URL') public baseUrl: string) {        
  }

  ngOnInit(): void {    
    this.updateData("PT30M", "PT1M");    
  }
  private updateData(timespan: string, interval:string) {        
    this.http.get<PerformanceData>(this.baseUrl + 'performance?timespan=' + timespan + "&interval=" + interval).subscribe(result => {
      this.lineChartData = [];        
      for (let dat of result.requests)
        this.lineChartData.push({ data: dat.map(p => { return { x: Date.parse(p.timestamp), y: Math.floor(p.timeTakenMs) }; }), label: dat[0].requestName, backgroundColor: stc(dat[0].requestName) +'77' });
    });
  }
  onChange(value) {
    console.log(value);
    if (value == "30m") { this.updateData("PT30M", "PT5M"); }
    if (value == "1h") { this.updateData("PT1H", "PT5M"); }
    if (value == "3h") { this.updateData("PT3H", "PT5M"); }
    if (value == "6h") { this.updateData("PT6H", "PT10M"); }
    if (value == "12h") { this.updateData("PT12H", "PT15M"); }
    if (value == "24h") { this.updateData("PT24H", "PT30M"); }
    if (value == "3d") { this.updateData("PT3D", "PT2H"); }
    if (value == "7d") { this.updateData("PT7D", "PT2H"); }
  }
}
