import { api } from "@/lib/api";
import type { PagedTransactionsResponseDto } from "@/types/Transactions/transaction";

export interface FetchTransactionsParams {
  page?: number;
  pageSize?: number;
}

export function fetchTransactions(params?: FetchTransactionsParams) {
  return api
    .get<PagedTransactionsResponseDto>("api/transactions", {
      params: {
        page: params?.page ?? 1,
        pageSize: params?.pageSize ?? 20,
      },
    })
    .then((res) => res.data);
}
