
import type { CaseDecision} from "@/types/Cases/cases";
import type { Severity } from "@/types/alert";
import type { Status } from "@/types/status";

export interface CaseTableRow {
  id: string;
  transactionId: string;
  clientName: string;
  accountIdentifier: string;
  status: Status;
  decision: CaseDecision;
  analystName: string;
  severity: Severity;
  findingsCount: number;
  openedAtUtc: string;
  resolvedAt?: string | null;
  totalAmount: number;
  currencyCode: string;
}
