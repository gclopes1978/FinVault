export interface Transaction {
  id: string;
  symbol: string;
  type: string;
  quantity: number;
  price: number;
  total: number;
  source: string;
  transactionDate: string;
}