// src/services/account.services.ts
import { api } from "@/lib/api";

export interface AccountResponseDto {
  id: string;
  accountNumber: string;
  balance: number;
  currencyCode: string;
}

export async function fetchMyAccount(): Promise<AccountResponseDto> {
  const stored = localStorage.getItem("AUTH_STORAGE_KEY");
  if (!stored) throw new Error("No login data found");

  const { state } = JSON.parse(stored);
  const accountId = state?.accountId; // Here we expect the accountId to be in the login data

  if (!accountId) throw new Error("Account ID not found in login data");

  const res = await api.get(`/accounts/${accountId}/identifiers`);

  const accountData: AccountResponseDto = {
    id: accountId,
    accountNumber: res.data.accountNumber,
    balance: res.data.balance,
    currencyCode: res.data.currencyCode,
  };
  return accountData;
}
