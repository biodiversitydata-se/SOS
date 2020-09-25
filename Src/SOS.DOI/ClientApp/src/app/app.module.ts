import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { SearchComponent } from './search/search.component';
import { TopMenuComponent } from './top-menu/top-menu.component';
import { ViewComponent } from './view/view.component';

// Services
import {
  DoiService,
  HttpClientService
} from '../services';

@NgModule({
  declarations: [
    AppComponent,
    SearchComponent,
    TopMenuComponent,
    ViewComponent
],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: SearchComponent, pathMatch: 'full' },
      { path: ':prefix/:suffix', component: ViewComponent, pathMatch: 'prefix' } 
    ])
  ],
  providers: [
    DoiService,
    HttpClientService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
