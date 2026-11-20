export interface PensionFundMovement {
  id: string;
  date: string;
  quantity: number;
  unitPrice: number;
  valuation: number;
}

export interface PensionInsuranceMovement {
  movement: string;
  receipt: string | null;
  date: string;
  debit: number | null;
  credit: number | null;
  balance: number;
}