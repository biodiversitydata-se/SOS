import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import {
  DoiService
} from '../../services';

import { IMetadata, IResponse } from '../../models/datacite';

@Component({
  templateUrl: './view.component.html',
  styleUrls: ['./view.component.scss']
})
export class ViewComponent implements OnInit {
  _doiData: IMetadata;
  _fileUrl: string;

  constructor(
    private readonly _route: ActivatedRoute,
    private readonly _doiService: DoiService
  ) {

  }

  ngOnInit(): void {
    const prefix = this._route.snapshot.paramMap.get('prefix');
    const suffix = this._route.snapshot.paramMap.get('suffix'); 

    this._doiService.getDoiMetadata(prefix, suffix)
      .subscribe(
        data => {
          this._doiData = data;
        },
        err => {
          console.error(err);
        }
    );

    this._doiService.getURL(prefix, suffix)
      .subscribe(
        url => {
          this._fileUrl = url;
        },
        err => {
          console.error(err);
        }
      );
  }
}
