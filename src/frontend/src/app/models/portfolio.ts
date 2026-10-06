export interface Portfolio {
  id: number;
  symbol: string;
  isin: string | null;
  source: string;
  quantity: number;
  averageCost: number;
  currentPrice: number;
  currentValue: number;
  currency: string;
  returnPercentage: number;
  lastUpdated: string;
}