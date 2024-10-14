import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  invalidLogin: boolean;

  constructor(public http: HttpClient, @Inject('BASE_URL') public baseUrl: string, public router: Router) { }

  ngOnInit() {
   
  }
  login(form: NgForm) {
    const credentials = JSON.stringify(form.value);
    this.http.post(this.baseUrl + "authentication/login", credentials, {
      headers: new HttpHeaders({
        "Content-Type": "application/json"
      })
    }).subscribe(response => {
      const token = (<any>response).token;
      localStorage.setItem("jwt", token);
      this.invalidLogin = false;
      this.router.navigate(["/observation-viewer"]);
    }, err => {
      this.invalidLogin = true;
    });
  }
}
