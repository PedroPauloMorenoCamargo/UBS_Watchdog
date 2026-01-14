// src/services/account.services.ts
import { api } from "@/lib/api";

export interface AccountResponseDto {
  id: string;
  accountNumber: string;
  balance: number;
  currencyCode: string;
}

export async function fetchMyAccount(): Promise<AccountResponseDto> {
  // Supondo que você tenha o accountId do login armazenado no token
  const stored = localStorage.getItem("AUTH_STORAGE_KEY");
  if (!stored) throw new Error("No login data found");

  const { state } = JSON.parse(stored);
  const accountId = state?.accountId; // ⚠️ aqui é fundamental ter o accountId no login

  if (!accountId) throw new Error("Account ID not found in login data");

  const res = await api.get(`/accounts/${accountId}/identifiers`);
  // Mapear para AccountResponseDto
  const accountData: AccountResponseDto = {
    id: accountId,
    accountNumber: res.data.accountNumber,
    balance: res.data.balance,
    currencyCode: res.data.currencyCode,
  };
  return accountData;
}
