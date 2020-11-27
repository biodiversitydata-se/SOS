import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;  
  environment: string;
  constructor(public http: HttpClient, @Inject('BASE_URL') public baseUrl: string) {
  }
  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
  ngOnInit() {
    this.http.get<Environment>(this.baseUrl + 'hostingenvironment').subscribe(result => {
      this.environment = result.environment;
    }, error => console.error(error));  
  }
}
