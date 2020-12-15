import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { DoiListComponent } from './components/doi-list/doi-list.component';
import { PaginationComponent } from './components/pagination/pagination.component';
import { TopMenuComponent } from './components/top-menu/top-menu.component';

import { AboutComponent } from './about/about.component';
import { AppComponent } from './app.component';
import { ListComponent } from './list/list.component';
import { SearchComponent } from './search/search.component';
import { ViewComponent } from './view/view.component';

// Services
import {
  DoiService,
  HttpClientService
} from '../services';

@NgModule({
  declarations: [
    AboutComponent,
    AppComponent,
    DoiListComponent,
    ListComponent,
    PaginationComponent,
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
      { path: ':prefix/:suffix', component: ViewComponent, pathMatch: 'prefix' },
      { path: 'list', component: ListComponent, pathMatch: 'full' },
      { path: 'about', component: AboutComponent, pathMatch: 'full' },
    ])
  ],
  providers: [
    DoiService,
    HttpClientService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
