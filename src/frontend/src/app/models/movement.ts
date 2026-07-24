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

// Envelope returned by GET /api/v1/accounts (paginated, filtered, sorted server-side)
export interface PagedMovementsResponse {
  items: MovementDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// Response returned by GET /api/v1/accounts/totals
export interface MovementTotals {
  totalCredits: number;
  totalDebits: number;
  currentBalance: number | null;
}

// Which date field a date-range filter applies to
export type DateFilterField = 'transactionDate' | 'valueDate';

// Query parameters accepted by GET /api/v1/accounts and /api/v1/accounts/totals.
// Dates are sent as 'yyyy-MM-dd' strings.
export interface MovementQueryParams {
  page?: number;
  pageSize?: number;
  transactionDateFrom?: string;
  transactionDateTo?: string;
  valueDateFrom?: string;
  valueDateTo?: string;
}
