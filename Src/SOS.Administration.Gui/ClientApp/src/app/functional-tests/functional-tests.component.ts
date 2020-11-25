import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Message } from '@angular/compiler/src/i18n/i18n_ast';
import { Component, Inject, OnInit } from '@angular/core';
import { format, parseISO } from 'date-fns';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-functional-tests',
  templateUrl: './functional-tests.component.html',
  styleUrls: ['./functional-tests.component.scss']
})
export class FunctionalTestsComponent implements OnInit {
  functionalTests: FunctionalTest[] = [];
  messageList: TestMessage[] = [];
  http: HttpClient;
  loadingData: boolean = false;
  constructor(http: HttpClient, @Inject('BASE_URL') public baseUrl: string) {
    this.http = http;   
  }
  ngOnInit() {
    this.fetchTests();    
  }  
  private fetchTests() {
    this.loadingData = true;
    this.http.get<FunctionalTest[]>(this.baseUrl + 'tests').subscribe(result => {
      this.functionalTests = result;
      for (let test of this.functionalTests) {
        test.currentStatus = "Unknown";
      }
      this.loadingData = false;
    }, error => console.error(error));  
  }
  private runTests() {
    this.messageList = [];
    for (let test of this.functionalTests) {
      test.currentStatus = "Unknown";
    }
    for (let test of this.functionalTests) {
      this.messageList.push({ timestamp: new Date(), message: "Running test:'" + test.description + "'", type: "Info" });
      this.http.get<TestResults>('tests/' + test.route).subscribe(result => {
        if (result) {
          this.setTestStatus(test, result);
          for (let message of result.results) {
            this.messageList.push({ timestamp:new Date(), message: test.description + ': ' + message.result, type: message.status });
          }
        }
      }, error => this.messageList.push({ timestamp:new Date(),message: error.message, type: "Failed" }));
    }
  }
  setTestStatus(test: FunctionalTest, result: TestResults) {
    test.timeTakenMs = result.timeTakenMs;
    for (let res of result.results) {
      if (res.status != "Succeeded") {
        test.currentStatus = "Failed";
        return;
      }
    }
    test.currentStatus = "Succeeded";
  }
  getGroups() {
    let groups = [];
    for (let test of this.functionalTests) {
      if (!groups.includes(test.group)) {
        groups.push(test.group)
      }
    }
    return groups;
  }
  getTests(group) {    
    return this.functionalTests.filter(p => p.group == group);
  }
  getTestClass(type) {   
    if (type == "Succeeded") { return "list-group-item list-group-item-success"; }
    if (type == "Failed") { return "list-group-item list-group-item-danger"; }
    return "list-group-item"; 
  }
  getMessageClass(type) {
    if (type == "Succeeded") { return "list-group-item-success"; }
    if (type == "Failed") { return "list-group-item-danger"; }
    return "";
  }
  
  formatDate(params) {
  if (params) {
    return format(parseISO(params), 'HH:mm:ss');
  }
  else {
    return '';
  }
}
}
class TestMessage {
  type: string;
  message: string;
  timestamp: Date;
}
class TestResult {
  result: string;
  status: string;
}
class TestResults {
  testId: number;
  timeTakenMs: number;
  results: TestResult[];
}
interface FunctionalTest {
  group: string;
  description: string;
  route: string;
  timeTakenMs: number;
  currentStatus: string;
}
