import type { Severity } from "./alert";
import type { KYC } from "./kycstatus";

export interface Client {
  id: string;
  name: string;
  country: string;
  risk: Severity;
  kyc: KYC;
  alerts: number;
  balance: number;
  lastActivity: string;
}
