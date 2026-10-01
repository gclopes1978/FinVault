import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { InvestmentsService } from '../../services/investments';
import { Portfolio } from '../../models/portfolio';

@Component({
  selector: 'app-investments',
  imports: [],
  templateUrl: './investments.html',
  styleUrl: './investments.scss',
})
export class Investments implements OnInit {
  portfolio: Portfolio[] = [];

  constructor(
    private readonly investmentsService: InvestmentsService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadPortfolio();
  }

  private loadPortfolio(): void {
    this.investmentsService.getPortfolio().subscribe({
      next: (portfolio) => {
        this.portfolio = portfolio;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Failed to load investments portfolio', error);
      }
    });
  }
}