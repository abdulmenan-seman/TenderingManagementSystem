import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FeaturedTenders } from './featured-tenders';

describe('FeaturedTenders', () => {
  let component: FeaturedTenders;
  let fixture: ComponentFixture<FeaturedTenders>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeaturedTenders],
    }).compileComponents();

    fixture = TestBed.createComponent(FeaturedTenders);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
