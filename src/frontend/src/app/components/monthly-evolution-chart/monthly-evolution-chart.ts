import {
  AfterViewInit,
  Component,
  ElementRef,
  Input,
  OnDestroy,
  ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { MovementDto } from '../../models/movement';

Chart.register(...registerables);

// One data point in the aggregated chart: either a month (year view) or a day (year+month view)
interface AggregatedPoint {
  label: string;
  credits: number;
  debits: number;
}

const MONTH_LABELS = [
  'Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun',
  'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'
];

@Component({
  selector: 'app-monthly-evolution-chart',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './monthly-evolution-chart.html',
  styleUrl: './monthly-evolution-chart.scss'
})
export class MonthlyEvolutionChartComponent implements AfterViewInit, OnDestroy {
  private _movements: MovementDto[] = [];

  // Full movement list for the account; aggregation is derived from this on every change.
  // Implemented as a setter (rather than relying on ngOnChanges) so it also reacts when
  // assigned directly, e.g. from tests that don't go through a bound host template.
  @Input()
  public set movements(value: MovementDto[]) {
    this._movements = value ?? [];
    this.recomputeAvailableYears();
    this.renderChart();
  }

  public get movements(): MovementDto[] {
    return this._movements;
  }

  @ViewChild('chartCanvas') private readonly chartCanvasRef!: ElementRef<HTMLCanvasElement>;

  // Every calendar year present in the movement list, newest first
  public availableYears: number[] = [];

  // Currently selected year; null until movements are loaded
  public selectedYear: number | null = null;

  // Currently selected month (1-12); null means "whole year" (monthly aggregation)
  public selectedMonth: number | null = null;

  public readonly monthOptions = MONTH_LABELS.map((label, index) => ({ value: index + 1, label }));

  private chart: Chart | null = null;
  private viewReady = false;

  public ngAfterViewInit(): void {
    this.viewReady = true;
    this.renderChart();
  }

  public ngOnDestroy(): void {
    this.chart?.destroy();
  }

  // Rebuild the list of selectable years from the current movements, defaulting to the most recent one
  private recomputeAvailableYears(): void {
    const years = new Set(this.movements.map((m) => new Date(m.transactionDate).getFullYear()));
    this.availableYears = Array.from(years).sort((a, b) => b - a);

    if (this.selectedYear === null || !this.availableYears.includes(this.selectedYear)) {
      this.selectedYear = this.availableYears[0] ?? null;
    }
  }

  public onYearChange(): void {
    this.selectedMonth = null;
    this.renderChart();
  }

  public onMonthChange(): void {
    this.renderChart();
  }

  // Aggregate the loaded movements into the points shown on the chart, according to the
  // current year/month selection: monthly totals for the whole year, or daily totals for
  // a single selected month.
  public buildAggregatedPoints(): AggregatedPoint[] {
    if (this.selectedYear === null) {
      return [];
    }

    const inSelectedYear = this.movements.filter(
      (m) => new Date(m.transactionDate).getFullYear() === this.selectedYear
    );

    if (this.selectedMonth === null) {
      const points: AggregatedPoint[] = MONTH_LABELS.map((label) => ({ label, credits: 0, debits: 0 }));

      for (const movement of inSelectedYear) {
        const monthIndex = new Date(movement.transactionDate).getMonth();
        this.addToPoint(points[monthIndex], movement);
      }

      return points;
    }

    const daysInMonth = new Date(this.selectedYear, this.selectedMonth, 0).getDate();
    const points: AggregatedPoint[] = Array.from({ length: daysInMonth }, (_, i) => ({
      label: String(i + 1),
      credits: 0,
      debits: 0
    }));

    const inSelectedMonth = inSelectedYear.filter(
      (m) => new Date(m.transactionDate).getMonth() + 1 === this.selectedMonth
    );

    for (const movement of inSelectedMonth) {
      const dayIndex = new Date(movement.transactionDate).getDate() - 1;
      this.addToPoint(points[dayIndex], movement);
    }

    return points;
  }

  private addToPoint(point: AggregatedPoint, movement: MovementDto): void {
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

    const canvas = this.chartCanvasRef.nativeElement;
    const context = canvas.getContext('2d');
    if (!context) {
      // No 2D context available (e.g. running under a test environment without canvas support)
      return;
    }

    const points = this.buildAggregatedPoints();

    const config: ChartConfiguration = {
      type: 'bar',
      data: {
        labels: points.map((p) => p.label),
        datasets: [
          {
            label: 'Proveitos (Créditos)',
            data: points.map((p) => p.credits),
            backgroundColor: '#16a34a'
          },
          {
            label: 'Custos (Débitos)',
            data: points.map((p) => p.debits),
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

    if (this.chart) {
      this.chart.data = config.data;
      this.chart.update();
    } else {
      this.chart = new Chart(context, config);
    }
  }
}
