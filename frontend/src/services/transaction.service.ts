// src/services/transaction.service.ts
import { api } from "@/lib/api";
import type {
  PagedTransactionsResponseDto,
  TransactionResponseDto,
  CreateTransactionDto,
  ImportTransactionsCsvResponseDto,
} from "@/types/Transactions/transaction";

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

export function fetchTransactionById(id: string) {
  return api
    .get<TransactionResponseDto>(`api/transactions/${id}`)
    .then((res) => res.data);
}

export function createTransaction(data: CreateTransactionDto) {
  return api
    .post<TransactionResponseDto>("api/transactions", data)
    .then((res) => res.data);
}

// export function updateTransaction(
//   id: string,
//   data: Partial<CreateTransactionDto>
// ) {
//   return api
//     .put<TransactionResponseDto>(`api/transactions/${id}`, data)
//     .then((res) => res.data);
// }

// export function deleteTransaction(id: string) {
//   return api.delete(`api/transactions/${id}`).then((res) => res.data);
// }

export function importTransactionsCSV(file: File) {
  const formData = new FormData();
  formData.append("file", file);

  return api
    .post<ImportTransactionsCsvResponseDto>("api/transactions/import", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    })
    .then((res) => res.data);
}