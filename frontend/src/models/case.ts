// models/case.models.ts
import type { CaseStatus, CaseDecision, CaseSeverity } from "@/types/Cases/cases";

export interface CaseTableRow {
  id: string;
  transactionId: string;
  clientName: string;
  accountIdentifier: string;
  status: CaseStatus;
  decision: CaseDecision;
  analystName: string;
  severity: CaseSeverity;
  findingsCount: number;
  openedAt: string;
  resolvedAt?: string | null;
}
