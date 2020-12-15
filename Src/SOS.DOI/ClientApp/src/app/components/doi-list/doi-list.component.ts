import { Component, Input } from '@angular/core';

import { IMetadata } from '../../../models/datacite';

@Component({
  selector: 'app-doi-list',
  templateUrl: './doi-list.component.html',
  styleUrls: ['./doi-list.component.scss']
})
export class DoiListComponent {
  @Input() visible: boolean;
  @Input() data: Array<IMetadata>;
}
