import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { LeafletModule } from '@asymmetrik/ngx-leaflet';
import { LeafletMarkerClusterModule } from '@asymmetrik/ngx-leaflet-markercluster';
import { AgGridModule } from 'ag-grid-angular';
import { ChartsModule, ThemeService } from 'ng2-charts';
import { GaugeModule } from 'angular-gauge';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { InvalidObservationsComponent } from './invalid-observations/invalid-observations.component';
import { InvalidGridComponent } from './invalid-grid/invalid-grid.component';
import { StatusComponent } from './status/status.component';
import { InvalidMapComponent } from './invalid-map/invalid-map.component';
import { FunctionalTestsComponent } from './functional-tests/functional-tests.component';
import { PerformanceChartComponent } from './performance-chart/performance-chart.component';
import { LoadTestComponent } from './load-test/load-test.component';
import { ObservationViewerComponent } from './observation-viewer/observation-viewer.component';
import { LoginComponent } from './login/login.component';
import { JwtModule } from '@auth0/angular-jwt';
import { AuthGuard } from './guard/authguard';

export function tokenGetter() {
  return localStorage.getItem("jwt");
}

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
    PerformanceChartComponent,
    LoadTestComponent,
    ObservationViewerComponent,
    LoginComponent,
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
      { path: 'loadtest', component: LoadTestComponent },
      { path: 'observation-viewer', component: ObservationViewerComponent, canActivate: [AuthGuard] },
      { path: 'login', component: LoginComponent },
    ]),
    LeafletModule,
    LeafletMarkerClusterModule,
    ChartsModule,
    AgGridModule.withComponents([]),
    GaugeModule.forRoot(),
    NgxDatatableModule,
    JwtModule.forRoot({
      config: {
        tokenGetter: tokenGetter,
        whitelistedDomains: ["localhost:44315", "sos-admin.artdata.slu.se","sos-admin-st.artdata.slu.se"],
        blacklistedRoutes: []
      }
    })
  ],
  providers: [ThemeService, AuthGuard],
  bootstrap: [AppComponent]
})
export class AppModule { }
