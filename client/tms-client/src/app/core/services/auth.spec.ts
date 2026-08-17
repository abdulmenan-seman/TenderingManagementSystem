import { TestBed } from '@angular/core/testing';

import * as AuthModule from './auth';

describe('Auth', () => {
  let service: any;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject((AuthModule as any).Auth ?? (AuthModule as any).default ?? Object.values(AuthModule)[0]);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
