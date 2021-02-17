import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;  
  environment: string;
  constructor(public http: HttpClient, @Inject('BASE_URL') public baseUrl: string, private jwtHelper: JwtHelperService, public router: Router) {
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
  logOut() {
    localStorage.removeItem("jwt");
    this.router.navigate(["/login"]);
  }
  isUserAuthenticated() {
    const token: string = localStorage.getItem("jwt");
    if (token && !this.jwtHelper.isTokenExpired(token)) {
      return true;
    }
    else {
      return false;
    }
  }
}
