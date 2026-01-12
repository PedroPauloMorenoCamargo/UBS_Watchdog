export type TransactionRow = {
  id: string;
  date: string;
  amount: string;
  rawAmount: number;
  type: string;
  method?: string;
  clientId: string;
  counterPartyName?: string;
  country?: string;
};
