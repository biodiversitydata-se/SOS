import { Component, Inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FileService } from '../services/file-service';
import { saveAs } from 'file-saver';

import * as moment from 'moment';

@Component({
  templateUrl: './protected-log.component.html'
})
export class ProtectedLogComponent {
  private _baseUrl: string;

  startDate: Date = null;
  endDate: Date = null;

  constructor(private datePipe: DatePipe, private _fileService: FileService, @Inject('BASE_URL') baseUrl: string) {
    moment.locale('sv', {
      longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'YYYY-MM-DD',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY [kl.] HH:mm',
        LLLL: 'dddd D MMMM YYYY [kl.] HH:mm',
        lll: 'D MMM YYYY HH:mm',
        llll: 'ddd D MMM YYYY HH:mm'
      }
    });

    this._baseUrl = baseUrl;
  }

  formatDate(date: Date): string {
    return this.datePipe.transform(date, "yyyy-MM-dd HH:mm")
  }

  onSearchFormSubmit(event) {

    this._fileService
      .downloadFile(`${this._baseUrl}protectedLog?from=${this.formatDate(this.startDate)}&to=${this.formatDate(this.endDate)}`)
      .subscribe(blob => saveAs(blob, 'logdata.csv')), (error: any) => console.log('Error downloading the file');
  }
}
