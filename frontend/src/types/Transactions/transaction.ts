export type TransactionTypeApi = 0 | 1 | 2;
export type TransferMethodApi = 0 | 1 | 2;
export type IdentifierTypeApi =
  | 0 // CPF
  | 1 // CNPJ
  | 2 // TAX_ID
  | 3 // PASSPORT
  | 4 // LEI
  | 5 // PIX_EMAIL
  | 6 // PIX_PHONE
  | 7 // PIX_RANDOM
  | 8 // IBAN
  | 9; // OTHER

export type TransactionResponseDto = {
  id: string;
  accountId: string;
  clientId: string;
  type: TransactionTypeApi;
  transferMethod?: TransferMethodApi | null;
  amount: number;
  currencyCode: string;
  baseCurrencyCode: string;
  baseAmount: number;
  fxRateId?: string | null;
  occurredAtUtc: string;

  cpName?: string | null;
  cpBank?: string | null;
  cpBranch?: string | null;
  cpAccount?: string | null;
  cpIdentifierType?: IdentifierTypeApi | null;
  cpIdentifier?: string | null;
  cpCountryCode?: string | null;

  createdAtUtc: string;
};

export interface PagedTransactionsResponseDto {
  items: TransactionResponseDto[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
}