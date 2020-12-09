import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import { LeafletMarkerClusterModule } from '@asymmetrik/ngx-leaflet-markercluster';
import { AgGridModule } from 'ag-grid-angular';
import { ChartsModule, ThemeService } from 'ng2-charts';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { InvalidObservationsComponent } from './invalid-observations/invalid-observations.component';
import { InvalidGridComponent } from './invalid-grid/invalid-grid.component';
import { StatusComponent } from './status/status.component';
import { InvalidMapComponent } from './invalid-map/invalid-map.component';
import { FunctionalTestsComponent } from './functional-tests/functional-tests.component';
import { PerformanceChartComponent } from './performance-chart/performance-chart.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    InvalidObservationsComponent,
    InvalidGridComponent,
    StatusComponent,
    InvalidMapComponent,
    FunctionalTestsComponent,
    PerformanceChartComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: StatusComponent, pathMatch: 'full' },
      { path: 'invalid-observations', component: InvalidObservationsComponent },
      { path: 'status', component: StatusComponent },
      { path: 'test', component: FunctionalTestsComponent },
      { path: 'stats', component: PerformanceChartComponent },
    ]),
    LeafletModule,
    LeafletMarkerClusterModule,
    ChartsModule,
    AgGridModule.withComponents([])
  ],
  providers: [ThemeService],
  bootstrap: [AppComponent]
})
export class AppModule { }