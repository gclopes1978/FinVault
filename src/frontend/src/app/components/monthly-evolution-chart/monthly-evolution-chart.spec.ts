import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MonthlyEvolutionChartComponent } from './monthly-evolution-chart';
import { MovementDto } from '../../models/movement';

describe('MonthlyEvolutionChartComponent', () => {
  let component: MonthlyEvolutionChartComponent;
  let fixture: ComponentFixture<MonthlyEvolutionChartComponent>;

  const movement = (overrides: Partial<MovementDto>): MovementDto => ({
    id: overrides.id ?? 'x',
    amount: overrides.amount ?? 0,
    type: overrides.type ?? 'C',
    balance: overrides.balance ?? 0,
    description: overrides.description ?? '',
    userNote: null,
    transactionDate: overrides.transactionDate ?? '2026-01-01T00:00:00Z',
    valueDate: overrides.transactionDate ?? '2026-01-01T00:00:00Z',
    createdAt: overrides.transactionDate ?? '2026-01-01T00:00:00Z'
  });

  const mockMovements: MovementDto[] = [
    movement({ id: 'm1', amount: 1000, type: 'C', transactionDate: '2026-01-05T10:00:00Z' }),
    movement({ id: 'm2', amount: 200, type: 'D', transactionDate: '2026-01-20T10:00:00Z' }),
    movement({ id: 'm3', amount: 300, type: 'D', transactionDate: '2026-02-10T10:00:00Z' }),
    movement({ id: 'm4', amount: 50, type: 'C', transactionDate: '2025-12-15T10:00:00Z' })
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MonthlyEvolutionChartComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(MonthlyEvolutionChartComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should derive the available years and default to the most recent one', () => {
    component.movements = mockMovements;
    fixture.detectChanges();

    expect(component.availableYears).toEqual([2026, 2025]);
    expect(component.selectedYear).toBe(2026);
  });

  it('should aggregate credits and debits per month for the whole selected year', () => {
    component.movements = mockMovements;
    fixture.detectChanges();

    const points = component.buildAggregatedPoints();
    expect(points).toHaveLength(12);
    expect(points[0]).toEqual({ label: 'Jan', credits: 1000, debits: 200 });
    expect(points[1]).toEqual({ label: 'Fev', credits: 0, debits: 300 });
  });

  it('should aggregate credits and debits per day when a specific month is selected', () => {
    component.movements = mockMovements;
    fixture.detectChanges();

    component.selectedMonth = 1;
    const points = component.buildAggregatedPoints();

    expect(points).toHaveLength(31);
    expect(points[4]).toEqual({ label: '5', credits: 1000, debits: 0 });
    expect(points[19]).toEqual({ label: '20', credits: 0, debits: 200 });
  });

  it('should reset the month filter when the year changes', () => {
    component.movements = mockMovements;
    fixture.detectChanges();

    component.selectedMonth = 3;
    component.onYearChange();

    expect(component.selectedMonth).toBeNull();
  });

  it('should return no points when there are no movements', () => {
    component.movements = [];
    fixture.detectChanges();

    expect(component.availableYears).toEqual([]);
    expect(component.selectedYear).toBeNull();
    expect(component.buildAggregatedPoints()).toEqual([]);
  });
});
