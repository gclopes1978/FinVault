import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef,
  inject,
  Input,
  OnDestroy,
  ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import Chart from 'chart.js/auto';
import { ChartConfiguration } from 'chart.js';

import { MovementDto, MovementTotalsByYearMonth } from '../../models/movement';

import { AccountService } from '../../services/account.service';

interface AggregatedPoint {
  label: string;
  credits: number;
  debits: number;
}

const MONTH_LABELS = [
  'Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun',
  'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'
];

// Movements requested for the per-day breakdown shown when a single month is selected.
// This full-history request is issued lazily: the yearly view is served by the server-side
// aggregates, so while no month is picked it never runs at all.
const DETAIL_PAGE_SIZE = 2000;

@Component({
  selector: 'app-monthly-evolution-chart',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './monthly-evolution-chart.html',
  styleUrl: './monthly-evolution-chart.scss'
})
export class MonthlyEvolutionChartComponent
implements AfterViewInit, OnDestroy {
  
  constructor(
    private readonly accountService: AccountService
  ) {}
  
  private readonly cdr = inject(ChangeDetectorRef);
  private movements: MovementDto[] = [];
  private movementsLoaded = false;
  private movementsLoading = false;
  private movementsRequestId = 0;

  @ViewChild('chartCanvas', { static: false })
  private chartCanvasRef?: ElementRef<HTMLCanvasElement>;

  private chart: Chart | null = null;
  private viewReady = false;
  private yearTotals: MovementTotalsByYearMonth[] = [];

  // Year the currently held totals belong to, so the render can tell whether the data on hand
  // matches the year on display
  private loadedYear: number | null = null;

  // Identifies the latest totals request so stale responses can be discarded
  private totalsRequestId = 0;

  private _dataVersion = 0;

  /**
   * Bumped by the parent whenever the underlying movements change (for example after a bank
   * statement import), so the lazily loaded detail history is discarded and refetched the next
   * time it is needed.
   */
  @Input()
  public set dataVersion(value: number) {
    if (value === this._dataVersion) {
      return;
    }

    this._dataVersion = value;
    this.discardMovements();

    // The statement that triggered this may belong to the year on display, so the aggregates
    // are refetched too, not just the detail history
    if (this.selectedYear !== null) {
      this.loadTotals(this.selectedYear);
    }
  }

  public availableYears: number[] = [];
  public selectedYear: number | null = null;
  public selectedMonth: number | null = null;

  public readonly monthOptions = MONTH_LABELS.map(
    (label, index) => ({
      value: index + 1,
      label
    })
  );

  public ngAfterViewInit(): void {
    this.viewReady = true;

    this.loadInitialData();
  }

  public ngOnDestroy(): void {
    this.chart?.destroy();
    this.chart = null;
  }

  /**
   * Loads the year list and the chart data at the same time.
   * The two requests used to run back to back because the totals call was chained to the
   * years response, which doubled the time to first paint. Since the dropdown always defaults
   * to the most recent year, the current calendar year is requested optimistically in parallel;
   * if the dropdown ends up on a different year, a second totals request replaces it.
   */
  private loadInitialData(): void {
    const optimisticYear = new Date().getFullYear();

    this.loadTotals(optimisticYear);

    this.accountService.getAvailableYears().subscribe({
      next: (years) => {
        this.availableYears = years;

        if (
          this.selectedYear === null ||
          !this.availableYears.includes(this.selectedYear)
        ) {
          this.selectedYear = this.availableYears[0] ?? null;
        }
      // Force the template to re-evaluate the year dropdown
        this.cdr.markForCheck();

        if (this.selectedYear === null) {
          return;
        }

        // Only refetch when the dropdown settled on a year the optimistic call did not cover.
        // renderIfYearMatches covers the other case: the optimistic response already arrived
        // before this handler ran, so the data is on hand and only the repaint is missing.
        if (this.selectedYear !== optimisticYear) {
          this.loadTotals(this.selectedYear);
        } else {
          this.renderIfYearMatches();
        }
      },
      error: (error) => {
        console.error('Erro ao carregar os anos disponíveis:', error);
        this.availableYears = [];
        this.selectedYear = null;
      }
    });
  }

  /**
   * Fetches the monthly totals for a year.
   * A request token guards against out-of-order responses: only the latest request may write
   * data, so a slow previous response cannot overwrite a newer one. The response is stored even
   * when it arrives before the dropdown has settled on a year (the initial request is issued
   * optimistically), and rendering is left to renderIfYearMatches.
   */
  private loadTotals(year: number): void {
    const requestId = ++this.totalsRequestId;

    this.accountService
      .getTotalsByYearMonth(year)
      .subscribe({
        next: (totals) => {
          if (requestId !== this.totalsRequestId) {
            return;
          }

          this.yearTotals = totals;
          this.loadedYear = year;

          this.renderIfYearMatches();
        },
        error: (error) => {
          console.error('Erro ao carregar os totais do ano:', error);
        }
      });
  }

  // Paints only when the totals on hand belong to the year currently on display
  private renderIfYearMatches(): void {
    if (this.loadedYear === this.selectedYear) {
      this.scheduleRender();
    }
  }

  public onYearChange(): void {
    this.selectedMonth = null;

    if (this.selectedYear === null) {
      return;
    }

    this.loadTotals(this.selectedYear);
  }


  /**
   * Loads the full movement history on demand, for the per-day breakdown of a selected month.
   * Guarded so repeated month switches reuse the already loaded history instead of refetching.
   */
  private ensureMovementsLoaded(): void {
    if (this.movementsLoaded) {
      this.scheduleRender();
      return;
    }

    if (this.movementsLoading) {
      return;
    }

    const requestId = ++this.movementsRequestId;

    this.movementsLoading = true;

    this.accountService
      .getMovements({ page: 1, pageSize: DETAIL_PAGE_SIZE })
      .subscribe({
        next: (response) => {
          if (requestId !== this.movementsRequestId) {
            return;
          }

          this.movements = response.items;
          this.movementsLoaded = true;
          this.movementsLoading = false;

          this.scheduleRender();
        },
        error: (error) => {
          if (requestId !== this.movementsRequestId) {
            return;
          }

          this.movementsLoading = false;
          console.error('Erro ao carregar os movimentos para o detalhe mensal:', error);
        }
      });
  }

  // Drops the cached detail history, invalidating any in-flight request for the previous version
  private discardMovements(): void {
    this.movements = [];
    this.movementsLoaded = false;
    this.movementsLoading = false;
    this.movementsRequestId++;
  }

  /**
   * Defers the repaint by one macrotask so the canvas ViewChild is resolved by change detection
   * before Chart.js is constructed against it.
   */
  private scheduleRender(): void {
    setTimeout(() => this.renderChart(), 0);
  }

  private buildYearPoints(
    totals: MovementTotalsByYearMonth[]
  ): AggregatedPoint[] {
    return MONTH_LABELS.map((label, index) => {
      const month = index + 1;

      const total = totals.find(
        x => x.month === month
      );

      return {
        label,
        credits: total?.totalCredit ?? 0,
        debits: total?.totalDebit ?? 0
      };
    });
  }

  public buildAggregatedPoints(): AggregatedPoint[] {
    if (this.selectedYear === null) {
      return [];
    }

    const inSelectedYear = this.movements.filter(
      m =>
        new Date(m.transactionDate).getFullYear() ===
        this.selectedYear
    );

    // Ano completo → agrupar por mês
    if (this.selectedMonth === null) {
      const points: AggregatedPoint[] = MONTH_LABELS.map(
        label => ({
          label,
          credits: 0,
          debits: 0
        })
      );

      for (const movement of inSelectedYear) {
        const monthIndex =
          new Date(movement.transactionDate).getMonth();

        this.addToPoint(
          points[monthIndex],
          movement
        );
      }

      return points;
    }

    // Mês selecionado → agrupar por dia
    const daysInMonth = new Date(
      this.selectedYear,
      this.selectedMonth,
      0
    ).getDate();

    const points: AggregatedPoint[] =
      Array.from(
        { length: daysInMonth },
        (_, i) => ({
          label: String(i + 1),
          credits: 0,
          debits: 0
        })
      );

    const inSelectedMonth =
      inSelectedYear.filter(
        m =>
          new Date(m.transactionDate).getMonth() + 1 ===
          this.selectedMonth
      );

    for (const movement of inSelectedMonth) {
      const dayIndex =
        new Date(movement.transactionDate).getDate() - 1;

      this.addToPoint(
        points[dayIndex],
        movement
      );
    }

    return points;
  }

  private addToPoint(
    point: AggregatedPoint,
    movement: MovementDto
  ): void {
    if (movement.type === 'C') {
      point.credits += movement.amount;
    } else {
      point.debits += movement.amount;
    }
  }

  private renderChart(): void {
    if (!this.viewReady || !this.chartCanvasRef) {
      return;
    }

    // Never paint totals that belong to a year other than the one on display
    if (this.loadedYear !== this.selectedYear) {
      return;
    }

    const canvas = this.chartCanvasRef.nativeElement;
    const context = canvas.getContext('2d');

    if (!context) {
      return;
    }

    const points =
      this.selectedMonth === null
        ? this.buildYearPoints(this.yearTotals)
        : this.buildAggregatedPoints();

    const config: ChartConfiguration = {
      type: 'bar',

      data: {
        labels: points.map(p => p.label),

        datasets: [
          {
            label: 'Proveitos (Créditos)',
            data: points.map(p => p.credits),
            backgroundColor: '#16a34a'
          },
          {
            label: 'Custos (Débitos)',
            data: points.map(p => p.debits),
            backgroundColor: '#dc2626'
          }
        ]
      },

      options: {
        responsive: true,
        maintainAspectRatio: false,

        scales: {
          y: {
            beginAtZero: true
          }
        }
      }
    };

    // Chart.js cannot reuse a canvas that already holds an instance, so the previous
    // chart is torn down before rebuilding instead of calling chart.update()
    this.chart?.destroy();

    this.chart = new Chart(
      context,
      config
    );
  }
}
