import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class FileService {

  constructor(private http: HttpClient) { }

  /**
   * Download a file
   * @param url
   */
  downloadFile(url: string): Observable<Blob> {
    return this.http.get(url, {
      responseType: 'blob'
    });
  }
}
