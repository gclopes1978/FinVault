import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { InvestmentsService } from '../../services/investments';
import { Portfolio } from '../../models/portfolio';
@Component({
  selector: 'app-investments',
  imports: [DecimalPipe],
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
        this.portfolio = portfolio.map((investment) => ({
          ...investment,
          returnPercentage:
            investment.averageCost > 0
              ? ((investment.currentValue -
                  investment.quantity * investment.averageCost) /
                  (investment.quantity * investment.averageCost)) *
                100
              : 0
        }));

        portfolio.forEach((investment) => {
          const marketSymbol = investment.symbol;
          console.log('Portfolio recebido:', portfolio);

          console.log(
            'IGLN recebido:',
            portfolio.find(x => x.symbol === 'IGLN.UK')
          );
          console.log(
            `Symbol: ${investment.symbol} -> Market symbol: ${marketSymbol}`
          );

          this.loadMarketPrice(investment, marketSymbol);
        });

        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Failed to load investments portfolio', error);
      }
    });
  }

  private loadMarketPrice(investment: Portfolio, marketSymbol: string): void {
    const assetType = 'ETF';

    this.investmentsService
      .getMarketPrice(assetType, marketSymbol)
      .subscribe({
        next: (quote) => {
          const investmentIndex = this.portfolio.findIndex(
            item =>
              item.symbol === investment.symbol &&
              item.source === investment.source
          );

          if (investmentIndex === -1) {
            return;
          }

          if (quote.currency === 'EUR') {
            const currentValue =
              this.portfolio[investmentIndex].quantity * quote.price;

            const costBasis =
              this.portfolio[investmentIndex].quantity *
              this.portfolio[investmentIndex].averageCost;

            this.portfolio[investmentIndex] = {
              ...this.portfolio[investmentIndex],
              currentPrice: quote.price,
              currentValue,
              returnPercentage:
                costBasis > 0
                  ? ((currentValue - costBasis) / costBasis) * 100
                  : 0,
              currency: 'EUR'
            };

            this.changeDetectorRef.detectChanges();
            return;
          }

          this.investmentsService
            .getExchangeRate(quote.currency, 'EUR')
            .subscribe({
              next: (exchangeRate) => {
                const currentPrice = quote.price * exchangeRate;

                const currentValue =
                  this.portfolio[investmentIndex].quantity * currentPrice;

                const costBasis =
                  this.portfolio[investmentIndex].quantity *
                  this.portfolio[investmentIndex].averageCost;

                this.portfolio[investmentIndex] = {
                  ...this.portfolio[investmentIndex],
                  currentPrice,
                  currentValue,
                  returnPercentage:
                    costBasis > 0
                      ? ((currentValue - costBasis) / costBasis) * 100
                      : 0,
                  currency: 'EUR'
                };

                this.changeDetectorRef.detectChanges();
              },
              error: (error) => {
                console.error(
                  `Failed to convert ${quote.currency} to EUR for ${investment.symbol}`,
                  error
                );
              }
            });
        },
        error: (error) => {
          console.error(
            `Failed to load market price for ${investment.symbol}`,
            error
          );
        }
      });
  }
}