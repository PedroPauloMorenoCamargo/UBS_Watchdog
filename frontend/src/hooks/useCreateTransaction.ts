import { useState } from "react";
import { api } from "@/lib/api";
import type { TransactionResponseDto, TransactionTypeApi, TransferMethodApi, IdentifierTypeApi } from "@/types/Transactions/transaction";

export interface CreateTransactionPayload {
  accountId: string;
  type: TransactionTypeApi;
  amount: number;
  currencyCode: string;
  transferMethod?: TransferMethodApi;
  cpName?: string;
  cpBank?: string;
  cpBranch?: string;
  cpAccount?: string;
  cpIdentifierType?: IdentifierTypeApi;
  cpIdentifier?: string;
  cpCountryCode?: string;
}

export function useCreateTransaction() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const submit = async (payload: CreateTransactionPayload) => {
    setLoading(true);
    setError(null);

    try {
      const requestPayload = {
        accountId: payload.accountId,
        type: payload.type,
        amount: payload.amount,
        currencyCode: payload.currencyCode,
        occurredAtUtc: new Date().toISOString(),
        transferMethod: payload.transferMethod,
        cpName: payload.cpName,
        cpBank: payload.cpBank,
        cpBranch: payload.cpBranch,
        cpAccount: payload.cpAccount,
        cpIdentifierType: payload.cpIdentifierType,
        cpIdentifier: payload.cpIdentifier,
        cpCountryCode: payload.cpCountryCode,
      };

      const response = await api.post<TransactionResponseDto>(
        "/api/transactions",
        requestPayload
      );

      return { success: true, data: response.data };
    } catch (err: any) {
      const errorMessage =
        err.response?.data?.message ||
        err.message ||
        "Failed to create transaction";
      setError(errorMessage);
      return { success: false, error: errorMessage };
    } finally {
      setLoading(false);
    }
  };

  return { submit, loading, error };
}
