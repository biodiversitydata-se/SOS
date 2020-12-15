import { Component } from '@angular/core';

import { DoiService } from '../../services';
import { IMetadata } from '../../models/datacite';

@Component({
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent {
  _doiList: Array<IMetadata>;
  _pageSize: number;
  _total: number;
  _pageIndex: number;

  /** Get batch of doi's */
  private getBatchAsync(): void {
    this._doiService.getBatchAsync(this._pageSize, this._pageIndex, 'created', 'desc')
      .subscribe(
        response => {
          this._total = response.meta.total;
          this._doiList = response.data;
        },
        err => {
          console.error(err);
        }
      );
  }

  /**
   * Constructor
   * @param _doiService
   */
  constructor(private readonly _doiService: DoiService) {
    this._pageSize = 10;
    this._pageIndex = 1;
    this.getBatchAsync();
  }

  onPageSizeChange(pageSize: number): void {
    this._pageIndex = 1;
    this._pageSize = pageSize;
    this.getBatchAsync();
  }

  /**
   * Event fired on paging
   * @param pageIndex
   */
  onPaging(pageIndex: number) : void {
    this._pageIndex = pageIndex;
    this.getBatchAsync();
  }
}
