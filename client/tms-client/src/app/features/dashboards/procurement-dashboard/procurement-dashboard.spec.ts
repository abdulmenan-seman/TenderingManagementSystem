import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProcurementDashboard } from './procurement-dashboard';

describe('ProcurementDashboard', () => {
  let component: ProcurementDashboard;
  let fixture: ComponentFixture<ProcurementDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProcurementDashboard],
    }).compileComponents();

    fixture = TestBed.createComponent(ProcurementDashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
