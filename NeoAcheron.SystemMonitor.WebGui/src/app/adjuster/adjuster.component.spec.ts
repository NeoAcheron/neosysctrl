import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AdjusterComponent } from './adjuster.component';

describe('AdjusterComponent', () => {
  let component: AdjusterComponent;
  let fixture: ComponentFixture<AdjusterComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AdjusterComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AdjusterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
