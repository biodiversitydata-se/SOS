import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { HttpClientService } from './httpclient.service';

import { IMetadata, IResponse } from '../models/datacite';


@Injectable()
export class DoiService {
  constructor(private readonly _httpClientService: HttpClientService) { }

  /**
   * Get batch of DOI's
   * @param take
   * @param page
   * @param orderBy
   * @param sortOrder
   */
  getBatchAsync(take: number, page: number, orderBy: string, sortOrder: string) : Observable<IResponse<Array<IMetadata>>> {
    return this._httpClientService.getAsync<IResponse<Array<IMetadata>>>(
      `doi?take=${take}&page=${page}&orderBy=${orderBy}&sortOrder=${sortOrder}`
    );
  }

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
  search(searchFor: string): Observable<IResponse<Array<IMetadata>>> {
    return this._httpClientService.getAsync<IResponse<Array<IMetadata>>>(
      `doi/search?searchFor=${ searchFor.replace(/\s/, '+') }`
    );
  }
}
