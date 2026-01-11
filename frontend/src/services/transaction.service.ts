import { api } from "@/lib/api";
import type { PagedTransactionsResponseDto } from "@/types/Transactions/transaction";

export function fetchTransactions() {
  return api
    .get<PagedTransactionsResponseDto>("api/transactions")
    .then((res) => res.data);
}
