import { Component, OnInit } from '@angular/core';
import * as mocha from 'mocha'

@Component({
  selector: 'app-functional-tests',
  templateUrl: './functional-tests.component.html',
  styleUrls: ['./functional-tests.component.scss']
})
export class FunctionalTestsComponent implements OnInit {

  constructor() { }

  ngOnInit() {
    mocha.setup('tdd')
  }

}
