export type TransactionRow = {
  id: string;
  date: string;
  amount: string;
  rawTimestamp: number ;
  rawAmount: number;
  type: string;
  method?: string;
  clientId: string;
  counterIdentifier?: string;
  country?: string;
  cpName?: string;
  currencyCode?: string;
};
