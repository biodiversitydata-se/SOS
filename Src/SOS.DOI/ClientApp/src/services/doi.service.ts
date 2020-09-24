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
  getDoiMetadata(prefix: string, suffix: string): Observable<IResponse<IMetadata>> {
    return this._httpClientService.getAsync<IResponse<IMetadata>>(
      `doi/${prefix}/${suffix}`
    );
  }

  getURL(doiSuffix: string): Observable<string> {
    doiSuffix = '30400d5e-7a34-4e1d-9669-2d054689d54f';
    return this._httpClientService.getString(
      `doi/${doiSuffix}/URL`
    );
  }

  /**
   * Search for a DOI
   * @param searchFor
   */
  search(searchFor: string): Observable<IResponse<Array<IMetadata>>> {
    return this._httpClientService.getAsync<IResponse<Array<IMetadata>>>(
      `doi/search?searchFor=${ searchFor.replace(/\s/, '+') }`
    );
  }
}
