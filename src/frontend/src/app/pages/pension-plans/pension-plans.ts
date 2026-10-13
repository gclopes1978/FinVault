import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import {
  PensionFundMovement,
  PensionInsuranceMovement
} from '../../models/pension-plan';
import { PensionPlansService } from '../../services/pension-plans';

@Component({
  selector: 'app-pension-plans',
  standalone: true,
  imports: [DatePipe, DecimalPipe],
  templateUrl: './pension-plans.html',
  styleUrl: './pension-plans.scss'
})
export class PensionPlans implements OnInit {
  fundMovements: PensionFundMovement[] = [];
  insuranceMovements: PensionInsuranceMovement[] = [];

  constructor(private readonly pensionPlansService: PensionPlansService, private readonly changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.loadMovements();
  }
  get latestFundMovement(): PensionFundMovement | undefined {
    console.log('Calculating latest fund movement:', this.fundMovements.length);
    return this.fundMovements.length > 0
      ? this.fundMovements[this.fundMovements.length - 1]
      : undefined;
  }
  get latestInsuranceMovement(): PensionInsuranceMovement | undefined {
    return this.insuranceMovements.reduce<PensionInsuranceMovement | undefined>(
      (latest, movement) => {
        if (!latest || movement.date > latest.date) {
          return movement;
        }

        return latest;
      },
      undefined
    );
  }
  private loadMovements(): void {
    this.pensionPlansService.getFundMovements().subscribe({
      next: (movements) => {
        console.log('Fund movements received:', movements);

        this.fundMovements = movements;
        console.log('fundMovements after update:', this.fundMovements);
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Failed to load pension fund movements.', error);
      }
    });

    this.pensionPlansService.getInsuranceMovements().subscribe({
      next: (movements) => {
        this.insuranceMovements = movements;
        this.changeDetectorRef.detectChanges();
      },
      error: (error) => {
        console.error('Failed to load pension insurance movements.', error);
      }
    });
  }
}