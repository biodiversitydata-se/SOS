import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FunctionalTestsComponent } from './functional-tests.component';

describe('FunctionalTestsComponent', () => {
  let component: FunctionalTestsComponent;
  let fixture: ComponentFixture<FunctionalTestsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FunctionalTestsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FunctionalTestsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
