import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { InvalidGridComponent } from './invalid-grid.component';

describe('InvalidGridComponent', () => {
  let component: InvalidGridComponent;
  let fixture: ComponentFixture<InvalidGridComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ InvalidGridComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InvalidGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
