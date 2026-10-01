import { TestBed } from '@angular/core/testing';

import { Investments } from './investments';

describe('Investments', () => {
  let service: Investments;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Investments);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
