import { Component } from '@angular/core';

import { DoiService } from '../../services';
import { IMetadata } from '../../models/datacite';

@Component({
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent {
  _searchTerm: string;
  _notFound: boolean;
  _doiSearch: Array<IMetadata>;
  _doiList: Array<IMetadata>;

  private resetSearch(): void {
    this._searchTerm = '';
  }

  private getLatest10Async(): void {
    this._doiService.getBatchAsync(10, 1, 'created', 'desc')
      .subscribe(
        response => {
          this._doiList = response.data;
        },
        err => {
          console.error(err);
        }
      );
  }

  constructor(private readonly _doiService: DoiService) {
    this._notFound = false;

    this.getLatest10Async();
  }

  onSearchClick(): void {
    this._doiService.search(this._searchTerm)
      .subscribe(
        response => {
          this._doiSearch = response.data;

          this._notFound = this._doiSearch.length === 0;

        },
        err => {
          console.error(err);
        }
      );

    this.resetSearch();
  }

  
}
