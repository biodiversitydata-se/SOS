import { Component } from '@angular/core';

import { DoiService } from '../../services';
import { IMetadata, IResponse } from '../../models/datacite';

@Component({
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent implements OnInit {
  _searchTerm: string;
  _notFound: boolean;
  _data: Array<IMetadata>;

  constructor(private readonly _doiService: DoiService) {
    this._notFound = false;
  }

  onSearchClick(): void {
    this._doiService.search(this._searchTerm)
      .subscribe(
        response => {
          this._data = response?.data;

          this._notFound = this._data?.length === 0;
        },
        err => {
          console.error(err);
        }
      );

    this.resetSearch();
  }

  private resetSearch(): void {
    this._searchTerm = '';
  }
}
