import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { HttpClientService } from './httpclient.service';

import { IMetadata, IResponse } from '../models/datacite';


@Injectable()
export class DoiService {
  constructor(private readonly _httpClientService: HttpClientService) { }

 /**
  * Get DOI meta data
  * @param prefix
  * @param suffix
  */
  getDoiMetadata(prefix: string, suffix: string): Observable<IMetadata> {
    return this._httpClientService.getAsync<IMetadata>(
      `doi/${prefix}/${suffix}`
    );
  }

  getURL(prefix: string, suffix: string): Observable<string> {
   
    return this._httpClientService.getString(
      `doi/${prefix}/${suffix}/URL`
    );
  }

  /**
   * Search for a DOI
   * @param searchFor
   */
  search(searchFor: string): Observable<Array<IMetadata>> {
    return this._httpClientService.getAsync<Array<IMetadata>>(
      `doi/search?searchFor=${ searchFor.replace(/\s/, '+') }`
    );
  }
}
