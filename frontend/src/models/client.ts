import type { Severity } from "@/types/alert";
import type { KYC } from "@/types/kycstatus";

export interface ClientTableRow {
  id: string;
  name: string;
  country: string;
  risk: Severity;
  kyc: KYC;
  alerts: number;
  balance: number;
  lastActivity: string;
}
