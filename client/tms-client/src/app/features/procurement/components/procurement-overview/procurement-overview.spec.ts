import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProcurementOverview } from './procurement-overview';

describe('ProcurementOverview', () => {
  let component: ProcurementOverview;
  let fixture: ComponentFixture<ProcurementOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProcurementOverview],
    }).compileComponents();

    fixture = TestBed.createComponent(ProcurementOverview);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
