import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TenderManagement } from './tender-management';

describe('TenderManagement', () => {
  let component: TenderManagement;
  let fixture: ComponentFixture<TenderManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TenderManagement],
    }).compileComponents();

    fixture = TestBed.createComponent(TenderManagement);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
