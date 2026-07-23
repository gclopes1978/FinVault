// Define a type-safe contract interface mapping the backend MovementResponse payload
export interface MovementDto {
  id: string;
  amount: number;
  // "C" (Crédito) or "D" (Débito), as returned by the CurrentAccount service
  type: 'C' | 'D';
  // Running account balance immediately after this movement was applied
  balance: number;
  description: string;
  userNote: string | null;
  transactionDate: string;
  valueDate: string;
  createdAt: string;
}
