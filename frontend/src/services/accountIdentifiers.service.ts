import { api } from "@/lib/api";
import type { AccountIdentifierDto, CreateAccountIdentifierRequest } from "@/types/AccountIdentifiers/accountIdentifier";

/**
 * Fetches all identifiers for a specific account.
 * @param accountId The unique identifier of the account.
 * @returns List of identifiers belonging to the account.
 */
export async function fetchAccountIdentifiers(accountId: string): Promise<AccountIdentifierDto[]> {
  const response = await api.get(`api/accounts/${accountId}/identifiers`);
  return response.data;
}

/**
 * Creates a new identifier for an account.
 * @param accountId The unique identifier of the account.
 * @param request The identifier creation request.
 * @returns The created identifier data.
 */
export async function createAccountIdentifier(
  accountId: string,
  request: CreateAccountIdentifierRequest
): Promise<AccountIdentifierDto> {
  // Backend expects PascalCase properties
  const payload = {
    IdentifierType: request.identifierType,
    IdentifierValue: request.identifierValue,
    IssuedCountryCode: request.issuedCountryCode || null,
  };
  
  console.log("Creating account identifier:", payload);
  
  const response = await api.post(`api/accounts/${accountId}/identifiers`, payload);
  return response.data;
}

/**
 * Removes an identifier from an account.
 * @param identifierId The unique identifier of the account identifier to remove.
 * @returns Promise that resolves when the identifier is removed.
 */
export async function deleteAccountIdentifier(identifierId: string): Promise<void> {
  await api.delete(`api/account-identifiers/${identifierId}`);
}
