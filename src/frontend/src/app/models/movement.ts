// Define a type-safe contract interface mapping out structural backend movement data transfer objects
export interface MovementDto {
  id: string;
  accountId: string;
  timestamp: string;
  description: string;
  type: 'Income' | 'Expense';
  amount: number;
  runningBalance: number;
}