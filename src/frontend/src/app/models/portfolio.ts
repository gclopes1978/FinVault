export interface Portfolio {
  id: number;
  symbol: string;
  source: string;
  quantity: number;
  averageCost: number;
  currentPrice: number;
  currentValue: number;
  lastUpdated: string;
}