import { api } from "@/lib/api";
import type { AccountResponseDto, CreateAccountRequest } from "@/types/Accounts/account";

/**
 * Fetches all accounts for a specific client.
 * @param clientId The unique identifier of the client.
 * @returns List of accounts belonging to the client.
 */
export async function fetchClientAccounts(clientId: string): Promise<AccountResponseDto[]> {
  const response = await api.get(`api/clients/${clientId}/accounts`);
  return response.data;
}

/**
 * Creates a new account for a client.
 * @param clientId The unique identifier of the client.
 * @param request The account creation request.
 * @returns The created account data.
 */
export async function createClientAccount(
  clientId: string,
  request: CreateAccountRequest
): Promise<AccountResponseDto> {
  // Backend expects PascalCase properties
  const payload = {
    AccountIdentifier: request.accountIdentifier,
    CountryCode: request.countryCode,
    AccountType: request.accountType,
    CurrencyCode: request.currencyCode,
  };
  
  console.log("Sending payload to API:", payload);
  console.log("Client ID:", clientId);
  
  const response = await api.post(`api/clients/${clientId}/accounts`, payload);
  return response.data;
}
