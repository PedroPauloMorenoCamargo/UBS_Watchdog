export const CaseStatus = {
  New: 0,
  UnderReview: 1,
  Resolved: 2,
} as const;

export type CaseStatus = (typeof CaseStatus)[keyof typeof CaseStatus];


export const CaseDecision = {
  Fraudulent: 0,
  NotFraudulent: 1,
  Inconclusive: 2,
} as const;
export type CaseDecision = (typeof CaseDecision)[keyof typeof CaseDecision];

export const CaseSeverity = {
  Low: 0,
  Medium: 1,
  High: 2,
  Critical: 3,
} as const;
export type CaseSeverity = (typeof CaseSeverity)[keyof typeof CaseSeverity];

export interface CaseResponseDto {
  id: string;
  transactionId: string;
  clientId: string;
  clientName: string;
  accountId: string;
  accountIdentifier: string;
  status: CaseStatus;
  decision: CaseDecision;
  analystId: string;
  analystName: string;
  severity: CaseSeverity;
  findingsCount: number;
  openedAt: Date;
  updatedAt: Date;
  resolvedAt?: Date | null;
}


export interface PagedCasesResponseDto {
  items: CaseResponseDto[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}