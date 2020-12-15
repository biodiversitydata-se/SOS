import { Component, EventEmitter, Injectable, Input, Output, OnChanges } from '@angular/core';

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.scss']
})
@Injectable()
export class PaginationComponent {
  visible: boolean;
  pages: number[];
  pageCount: number;

  @Input() pageSize: number;
  @Input() itemCount: number;
  @Input() maxPages: number;
  @Input() pageIndex: number;
  @Output() paging = new EventEmitter<number>();

  ngOnChanges(): void {
    this.pageCount = Math.ceil(this.itemCount / Number(this.pageSize));
    this.initializePagesNavigation();
  }

  onPageNavigationClick(currentPage: number): boolean {
    if (currentPage < 1) {
      currentPage = 1;
    } else if (currentPage > this.pageCount) {
      currentPage = this.pageCount;
    }

    this.pageIndex = currentPage;
    this.paging.emit(this.pageIndex);
    this.initializePagesNavigation();

    return false;
  }

  private initializePagesNavigation(): void {
    this.pages = [];
    this.visible = this.itemCount > this.pageSize;

    if (this.visible) {
      const maxPages = Number(this.maxPages);

      // Calculate start index
      let startIndex: number = this.pageIndex - Math.floor(maxPages / 2);

      // start index can't be less than 1
      if (startIndex < 1) {
        startIndex = 1;
        // If possible, make sure we show [maxPages] pages
      } else if ((startIndex + maxPages) > this.pageCount) {
        startIndex = this.pageCount - maxPages + 1 < 1 ? 1 : this.pageCount - maxPages + 1;
      }

      // Calculate end index
      const endIndex: number =
        startIndex + maxPages > this.pageCount ? this.pageCount : startIndex + maxPages - 1;

      // Populate array with page numbers to show
      while (startIndex <= endIndex) {
        this.pages.push(startIndex);
        startIndex++;
      }
    }
  }
}
