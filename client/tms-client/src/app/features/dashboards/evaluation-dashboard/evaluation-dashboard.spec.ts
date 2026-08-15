import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EvaluationDashboard } from './evaluation-dashboard';

describe('EvaluationDashboard', () => {
  let component: EvaluationDashboard;
  let fixture: ComponentFixture<EvaluationDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EvaluationDashboard],
    }).compileComponents();

    fixture = TestBed.createComponent(EvaluationDashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
