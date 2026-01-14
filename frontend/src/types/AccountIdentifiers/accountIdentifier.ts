// Account Identifier types matching backend

export const IdentifierType = {
  CPF: 0,
  CNPJ: 1,
  TAX_ID: 2,
  PASSPORT: 3,
  LEI: 4,
  PIX_EMAIL: 5,
  PIX_PHONE: 6,
  PIX_RANDOM: 7,
  IBAN: 8,
  OTHER: 9,
} as const;

export type IdentifierType = (typeof IdentifierType)[keyof typeof IdentifierType];

export interface AccountIdentifierDto {
  id: string;
  identifierType: IdentifierType;
  identifierValue: string;
  issuedCountryCode?: string | null;
  createdAtUtc: string;
}

export interface CreateAccountIdentifierRequest {
  identifierType: IdentifierType;
  identifierValue: string;
  issuedCountryCode?: string | null;
}

export const identifierTypeMap: Record<IdentifierType, string> = {
  [IdentifierType.CPF]: "CPF",
  [IdentifierType.CNPJ]: "CNPJ",
  [IdentifierType.TAX_ID]: "Tax ID",
  [IdentifierType.PASSPORT]: "Passport",
  [IdentifierType.LEI]: "LEI",
  [IdentifierType.PIX_EMAIL]: "PIX Email",
  [IdentifierType.PIX_PHONE]: "PIX Phone",
  [IdentifierType.PIX_RANDOM]: "PIX Random Key",
  [IdentifierType.IBAN]: "IBAN",
  [IdentifierType.OTHER]: "Other",
};
