import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { InvalidMapComponent } from '../invalid-map/invalid-map.component';

describe('InvalidMapComponent', () => {
  let component: InvalidMapComponent;
  let fixture: ComponentFixture<InvalidMapComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ InvalidMapComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InvalidMapComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
