import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { format, parseISO } from 'date-fns';
import { LogEntries } from '../models/logentries';
import { LogEntry } from '../models/logentry';
import { TermAggregation } from '../models/termaggregation';

@Component({
  selector: 'app-log-viewer',
  templateUrl: './log-viewer.component.html',
  styleUrls: ['./log-viewer.component.scss']
})
export class LogViewerComponent implements OnInit {
  logEntries: LogEntry[];
  aggregations: TermAggregation[];
  filters: { [name: string]: boolean } = {};
  includeDebug: boolean = false;
  includeInfo: boolean = true;
  includeError: boolean = true;
  includeWarning: boolean = true;
  loading: boolean;
  constructor(public http: HttpClient, @Inject('BASE_URL') public baseUrl: string) { }

  ngOnInit() {
    this.fetchLogs(true);
  }
  private fetchLogs(initFilters: boolean) {
    let skip = 0;
    let take = 100;
    this.loading = true;
    let filters = this.getFilters();
    this.http.get<LogEntries>(this.baseUrl + 'logs/latest?skip=' + skip + "&take=" + take + "&filters=" + filters).subscribe(result => {
      this.logEntries = result.logEntries;
      this.aggregations = result.aggregations;
      if (initFilters) {
        for (let agg of this.aggregations) {
          for (let term of agg.terms) {
            this.filters[agg.name + '_' + term.name] = true;
          }
        }
      }
      this.loading = false;
    }, error => console.error(error));
  }
  getFilters() {
    if (!this.aggregations) { return;}
    let filter = "";
    for (let agg of this.aggregations) {
      for (let term of agg.terms) {
        if (this.filters[agg.name + '_' + term.name]) {
          filter += agg.name + '_' + term.name + "," ;
        }
      }
    }
    return filter;
  }
  getLogEntryClass(level: string) {
    if (level == "Error") { return "list-group-item-danger"; }
    return "";
  }
  formatDate(params) {
    if (params) {
      return format(parseISO(params), 'yy-MM-dd HH:mm:ss');
    }
    else {
      return '';
    }
  }
  changeFilter(type: string) {
    this.filters[type] = !this.filters[type];
    this.fetchLogs(false);
  }
}
