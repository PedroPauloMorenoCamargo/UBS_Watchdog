export type TransactionRow = {
  id: string;
  date: string;
  amount: string;
  type: string;
  method?: string;
  clientId: string;
  counterPartyName?: string;
  country?: string;
};
