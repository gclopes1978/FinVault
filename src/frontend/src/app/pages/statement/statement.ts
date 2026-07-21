import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// Define a structured interface representing a financial movement or transaction row
export interface Transaction {
  id: string;
  date: Date;
  description: string;
  type: 'Income' | 'Expense';
  amount: number;
  balanceAfter: number;
}

@Component({
  selector: 'app-statement',
  standalone: true,
  // Import CommonModule for structural directives and FormsModule for two-way filter binding
  imports: [CommonModule, FormsModule],
  templateUrl: './statement.html',
  styleUrls: ['./statement.scss']
})
export class StatementComponent implements OnInit {
  // Store the master collection dataset representing all historical client transactions
  public allTransactions: Transaction[] = [];

  // Store the processed subset array displayed dynamically to the user inside the view table
  public filteredTransactions: Transaction[] = [];

  // Track the currently active selection criteria parameter filter value tied to the UI dropdown
  public selectedTypeFilter: string = 'All';

  // Initialize the component state mapping out a realistic mock sequence array database populate trace
  public ngOnInit(): void {
    this.allTransactions = [
      {
        id: 'TX-1001',
        date: new Date('2026-06-20T10:30:00'),
        description: 'Salary Monthly Deposit - TechCorp',
        type: 'Income',
        amount: 2500.00,
        balanceAfter: 2500.00
      },
      {
        id: 'TX-1002',
        date: new Date('2026-06-21T14:15:00'),
        description: 'Supermarket Continente Store Purchase',
        type: 'Expense',
        amount: 124.50,
        balanceAfter: 2375.50
      },
      {
        id: 'TX-1003',
        date: new Date('2026-06-22T09:00:00'),
        description: 'Galp Gas Station Fuel Refill',
        type: 'Expense',
        amount: 60.00,
        balanceAfter: 2315.50
      },
      {
        id: 'TX-1004',
        date: new Date('2026-06-23T18:45:00'),
        description: 'Freelance Mobile App Design Payment',
        type: 'Income',
        amount: 450.00,
        balanceAfter: 2765.50
      },
      {
        id: 'TX-1005',
        date: new Date('2026-06-24T11:20:00'),
        description: 'Netflix Portugal Monthly Subscription',
        type: 'Expense',
        amount: 15.99,
        balanceAfter: 2749.51
      }
    ];

    // Initialize the baseline filtered dataset variable matching the initial default global array state
    this.applyFilters();
  }

  // Execute internal calculation loops to filter data collections dynamically by operation constraints
  public applyFilters(): void {
    // Check if the interactive filter selection parameter requires a total aggregate overview match
    if (this.selectedTypeFilter === 'All') {
      // Transfer the entire comprehensive master layout database array down into the filtered subset
      this.filteredTransactions = [...this.allTransactions];
    } else {
      // Isolate distinct transaction rows matching specific criteria like Income or Expense tokens explicitly
      this.filteredTransactions = this.allTransactions.filter(
        (tx) => tx.type === this.selectedTypeFilter
      );
    }
  }

  // --- MÉTODOS ADICIONADOS AQUI PARA O HTML ---

  public getTypeClass(type: string): Record<string, boolean> {
    return {
      'badge-income': type === 'Income',
      'badge-expense': type === 'Expense'
    };
  }

  public getAmountClass(type: string): Record<string, boolean> {
    return {
      'amount-positive': type === 'Income',
      'amount-negative': type === 'Expense'
    };
  }
}