
// Chart.js is instantiated directly by the component, so it is replaced with a spy that records
// the configuration passed to the constructor
jest.mock('chart.js/auto', () => ({
  __esModule: true,
  default: jest.fn().mockImplementation(() => ({
    destroy: jest.fn(),
    update: jest.fn()
  }))
}));

import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { Subject } from 'rxjs';
import Chart, { ChartConfiguration } from 'chart.js/auto';

import { MonthlyEvolutionChartComponent } from './monthly-evolution-chart';
import {
  MovementDto,
  MovementTotalsByYearMonth,
  PagedMovementsResponse
} from '../../models/movement';
import { AccountService } from '../../services/account.service';

describe('MonthlyEvolutionChartComponent', () => {
  let component: MonthlyEvolutionChartComponent;
  let fixture: ComponentFixture<MonthlyEvolutionChartComponent>;

  // Emits on demand so the tests control the exact order in which the requests resolve
  let years$: Subject<number[]>;
  let totals$: Map<number, Subject<MovementTotalsByYearMonth[]>>;

  // One subject per call, so a test can resolve a specific request and leave the others open
  let movementRequests: Subject<PagedMovementsResponse>[];

  let getAvailableYears: jest.Mock;
  let getTotalsByYearMonth: jest.Mock;
  let getMovements: jest.Mock;

  const currentYear = new Date().getFullYear();

  const movement = (overrides: Partial<MovementDto>): MovementDto => ({
    id: overrides.id ?? 'x',
    amount: overrides.amount ?? 0,
    type: overrides.type ?? 'C',
    balance: overrides.balance ?? 0,
    description: overrides.description ?? '',
    userNote: null,
    transactionDate: overrides.transactionDate ?? `${currentYear}-01-05T10:00:00Z`,
    valueDate: overrides.valueDate ?? `${currentYear}-01-05T10:00:00Z`,
    createdAt: overrides.createdAt ?? `${currentYear}-01-05T10:00:00Z`
  });

  const totalsFor = (
    year: number,
    month: number,
    credit: number,
    debit: number
  ): MovementTotalsByYearMonth => ({
    year,
    month,
    totalCredit: credit,
    totalDebit: debit
  });

  // Reads back the configuration handed to the most recent Chart constructor call
  const lastChartConfig = (): ChartConfiguration => {
    const calls = (Chart as unknown as jest.Mock).mock.calls;

    return calls[calls.length - 1][1] as ChartConfiguration;
  };

  beforeEach(async () => {
    jest.clearAllMocks();

    years$ = new Subject<number[]>();
    totals$ = new Map<number, Subject<MovementTotalsByYearMonth[]>>();
    movementRequests = [];

    getAvailableYears = jest.fn(() => years$.asObservable());

    getTotalsByYearMonth = jest.fn((year: number) => {
      const subject = new Subject<MovementTotalsByYearMonth[]>();

      totals$.set(year, subject);

      return subject.asObservable();
    });

    getMovements = jest.fn(() => {
      const subject = new Subject<PagedMovementsResponse>();

      movementRequests.push(subject);

      return subject.asObservable();
    });

    await TestBed.configureTestingModule({
      imports: [MonthlyEvolutionChartComponent],
      providers: [
        {
          provide: AccountService,
          useValue: {
            getAvailableYears,
            getTotalsByYearMonth,
            getMovements
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MonthlyEvolutionChartComponent);
    component = fixture.componentInstance;
  });

  // Drives the component to the state where the yearly view is rendered and interactive, with
  // the totals response landing after the year list
  const initialise = (): void => {
    fixture.detectChanges();

    years$.next([currentYear]);
    years$.complete();

    totals$.get(currentYear)?.next([]);
    totals$.get(currentYear)?.complete();

    tick();
  };

  // Same end state, but with the parallel requests resolving the way they actually race in the
  // app: the optimistic totals response arrives while selectedYear is still null
  const initialiseWithTotalsArrivingFirst = (): void => {
    fixture.detectChanges();

    totals$.get(currentYear)?.next([]);
    totals$.get(currentYear)?.complete();

    years$.next([currentYear]);
    years$.complete();

    tick();
  };

  // Resolves the n-th movement-history request and leaves any other one open
  const resolveMovements = (
    index: number,
    items: MovementDto[]
  ): void => {
    movementRequests[index].next({
      items,
      page: 1,
      pageSize: 2000,
      totalCount: items.length,
      totalPages: 1
    });

    movementRequests[index].complete();
  };

  it('should create', () => {
    fixture.detectChanges();

    expect(component).toBeTruthy();
  });

  it('should request the year list and the totals of the current year in parallel', () => {
    fixture.detectChanges();

    // Both calls happen before the years response resolves, so neither waits on the other
    expect(getAvailableYears).toHaveBeenCalledTimes(1);
    expect(getTotalsByYearMonth).toHaveBeenCalledTimes(1);
    expect(getTotalsByYearMonth).toHaveBeenCalledWith(currentYear);
  });

  it('should default to the most recent year returned by the service', fakeAsync(() => {
    fixture.detectChanges();

    years$.next([currentYear, currentYear - 1]);
    years$.complete();
    tick();

    expect(component.availableYears).toEqual([
      currentYear,
      currentYear - 1
    ]);

    expect(component.selectedYear).toBe(currentYear);

    // The optimistic request already covered this year, so no second call is made
    expect(getTotalsByYearMonth).toHaveBeenCalledTimes(1);
  }));

  it('should render when the totals response arrives before the year list', fakeAsync(() => {
    initialiseWithTotalsArrivingFirst();

    expect(Chart).toHaveBeenCalled();
    expect(lastChartConfig().data.labels).toHaveLength(12);

    // Still a single totals request: the settled year matched the optimistic one
    expect(getTotalsByYearMonth).toHaveBeenCalledTimes(1);
  }));

  it('should render the data of the year the dropdown settles on when it differs', fakeAsync(() => {
    fixture.detectChanges();

    // Optimistic totals for the current year land first and are held, not discarded
    totals$.get(currentYear)?.next([
      totalsFor(currentYear, 1, 999, 999)
    ]);
    totals$.get(currentYear)?.complete();

    years$.next([currentYear - 1]);
    years$.complete();

    totals$.get(currentYear - 1)?.next([
      totalsFor(currentYear - 1, 1, 10, 5)
    ]);
    totals$.get(currentYear - 1)?.complete();

    tick();

    // The optimistic year's data must not leak into the chart
    expect(lastChartConfig().data.datasets?.[0].data?.[0]).toBe(10);
  }));

  it('should refetch the totals when the dropdown settles on another year', fakeAsync(() => {
    fixture.detectChanges();

    years$.next([currentYear - 1]);
    years$.complete();
    tick();

    expect(component.selectedYear).toBe(currentYear - 1);
    expect(getTotalsByYearMonth).toHaveBeenCalledTimes(2);
    expect(getTotalsByYearMonth).toHaveBeenLastCalledWith(
      currentYear - 1
    );
  }));

  it('should render the monthly totals of the selected year', fakeAsync(() => {
    fixture.detectChanges();

    years$.next([currentYear]);
    years$.complete();

    totals$.get(currentYear)?.next([
      totalsFor(currentYear, 1, 100, 40),
      totalsFor(currentYear, 2, 25, 0)
    ]);
    totals$.get(currentYear)?.complete();

    tick();

    const config = lastChartConfig();

    expect(config.data.labels).toHaveLength(12);

    // Months without data in the response are defaulted to zero
    expect(config.data.datasets?.[0].data).toEqual([
      100, 25, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    ]);

    expect(config.data.datasets?.[1].data).toEqual([
      40, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    ]);
  }));

  it('should discard a stale totals response that resolves after a newer request', fakeAsync(() => {
    fixture.detectChanges();

    years$.next([currentYear - 1]);
    years$.complete();
    tick();

    // The first (optimistic) request resolves late, after the second one already won
    totals$.get(currentYear)?.next([
      totalsFor(currentYear, 1, 999, 999)
    ]);
    totals$.get(currentYear)?.complete();

    totals$.get(currentYear - 1)?.next([
      totalsFor(currentYear - 1, 1, 10, 5)
    ]);
    totals$.get(currentYear - 1)?.complete();

    tick();

    const config = lastChartConfig();

    expect(config.data.datasets?.[0].data?.[0]).toBe(10);
    expect(config.data.datasets?.[1].data?.[0]).toBe(5);
  }));

  it('should keep the current selection when the user picks another year', fakeAsync(() => {
    fixture.detectChanges();

    years$.next([currentYear, currentYear - 1]);
    years$.complete();

    totals$.get(currentYear)?.next([]);
    totals$.get(currentYear)?.complete();

    tick();

    component.selectedYear = currentYear - 1;
    component.onYearChange();

    expect(getTotalsByYearMonth).toHaveBeenLastCalledWith(
      currentYear - 1
    );
  }));

  it('should clear the selection when the year list fails to load', fakeAsync(() => {
    const consoleErrorSpy = jest
      .spyOn(console, 'error')
      .mockImplementation(() => {});

    try {
      fixture.detectChanges();

      const error = new Error('boom');
      years$.error(error);
      tick();

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Erro ao carregar os anos disponíveis:',
        error
      );

      expect(component.availableYears).toEqual([]);
      expect(component.selectedYear).toBeNull();
    } finally {
      consoleErrorSpy.mockRestore();
    }
  }));

  describe('monthly detail movements', () => {
    it('should not request the movement history while no month is selected', fakeAsync(() => {
      initialise();

      expect(component.selectedMonth).toBeNull();
      expect(getMovements).not.toHaveBeenCalled();
    }));

    it('should refetch the yearly aggregates when the parent bumps the data version', fakeAsync(() => {
      initialise();

      expect(getTotalsByYearMonth).toHaveBeenCalledTimes(1);

      // Simulates a successful bank statement import
      component.dataVersion = 1;

      expect(getTotalsByYearMonth).toHaveBeenCalledTimes(2);
      expect(getTotalsByYearMonth).toHaveBeenLastCalledWith(
        currentYear
      );
    }));
  });
});

