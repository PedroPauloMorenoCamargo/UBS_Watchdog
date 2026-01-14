export interface ClientReportDto {
  clientId: string;
  clientName: string;
  countryCode: string;
  riskLevel: number;
  periodStart: string;
  periodEnd: string;
  transactionMetrics: TransactionMetrics;
  caseMetrics: CaseMetrics;
  transactionTrend: TransactionTrendItem[];
  casesBySeverity: SeverityItem[];
  transactionsByType: TransactionTypeItem[];
  topAccounts: TopAccountItem[];
}

export interface TransactionMetrics {
  totalTransactions: number;
  totalVolumeUSD: number;
  averageTransactionUSD: number;
  depositCount: number;
  withdrawalCount: number;
  transferCount: number;
}

export interface CaseMetrics {
  totalCases: number;
  newCases: number;
  underReviewCases: number;
  resolvedCases: number;
  fraudulentCases: number;
  notFraudulentCases: number;
  inconclusiveCases: number;
  lowSeverityCases: number;
  mediumSeverityCases: number;
  highSeverityCases: number;
  criticalSeverityCases: number;
}

export interface TransactionTrendItem {
  date: string;
  count: number;
  volumeUSD: number;
}

export interface SeverityItem {
  severity: string;
  count: number;
}

export interface TransactionTypeItem {
  type: string;
  count: number;
  volumeUSD: number;
}

export interface TopAccountItem {
  accountId: string;
  transactionCount: number;
  totalVolumeUSD: number;
}

export interface ClientDetailDto {
  id: string;
  legalType: number;
  name: string;
  contactNumber: string;
  addressJson: AddressJson | null;
  countryCode: string;
  riskLevel: number;
  kycStatus: number;
  createdAtUtc: string;
  updatedAtUtc: string;
  totalAccounts: number;
  totalTransactions: number;
  totalCases: number;
}

export interface AddressJson {
  city?: string;
  state?: string;
  street?: string;
  country?: string;
  zipCode?: string;
}
