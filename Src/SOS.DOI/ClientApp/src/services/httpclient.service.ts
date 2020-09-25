import { Injectable } from '@angular/core';
import { HttpClient  } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class HttpClientService {

  private readonly _baseUri: string = "/api/";

  constructor(private readonly http: HttpClient) { }

  getAsync<T>(uri: string): Observable<T> {
    return this.http.get<T>(encodeURI(`${this._baseUri}${uri}`));
  }

  getString(uri: string): Observable<string> {
    return this.http.get(encodeURI(`${this._baseUri}${uri}`), {
      responseType: 'text'});
  }

  postAsync<T>(uri: string, data: any): Observable<T> {
    return this.http.post<T>(encodeURI(`${this._baseUri}${uri}`), data);
  }

  putAsync<T>(uri: string, data: any): Observable<T> {
    return this.http.put<T>(encodeURI(`${this._baseUri}${uri}`), data);
  }

  deleteAsync<T>(uri: string): Observable<T> {
    return this.http.delete<T>(encodeURI(`${this._baseUri}${uri}`));
  }
}
