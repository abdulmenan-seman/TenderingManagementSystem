import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BidderDashboard } from './bidder-dashboard';

describe('BidderDashboard', () => {
  let component: BidderDashboard;
  let fixture: ComponentFixture<BidderDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BidderDashboard],
    }).compileComponents();

    fixture = TestBed.createComponent(BidderDashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
