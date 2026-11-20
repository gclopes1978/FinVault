import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PensionPlans } from './pension-plans';

describe('PensionPlans', () => {
  let component: PensionPlans;
  let fixture: ComponentFixture<PensionPlans>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PensionPlans],
    }).compileComponents();

    fixture = TestBed.createComponent(PensionPlans);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
