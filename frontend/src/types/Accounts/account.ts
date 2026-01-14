// Account types matching backend AccountContracts.cs

export const AccountType = {
  Checking: 0,
  Savings: 1,
  Investment: 2,
  Other: 3,
} as const;

export type AccountType = (typeof AccountType)[keyof typeof AccountType];

export const AccountStatus = {
  Active: 0,
  Blocked: 1,
  Closed: 2,
} as const;

export type AccountStatus = (typeof AccountStatus)[keyof typeof AccountStatus];

export interface AccountResponseDto {
  id: string;
  clientId: string;
  accountIdentifier: string;
  countryCode: string;
  accountType: AccountType;
  currencyCode: string;
  status: AccountStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateAccountRequest {
  accountIdentifier: string;
  countryCode: string;
  accountType: AccountType;
  currencyCode: string;
}

export const accountTypeMap: Record<AccountType, string> = {
  [AccountType.Checking]: "Checking",
  [AccountType.Savings]: "Savings",
  [AccountType.Investment]: "Investment",
  [AccountType.Other]: "Other",
};

export const accountStatusMap: Record<AccountStatus, string> = {
  [AccountStatus.Active]: "Active",
  [AccountStatus.Blocked]: "Blocked",
  [AccountStatus.Closed]: "Closed",
};
