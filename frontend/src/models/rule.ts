import type { Severity } from "@/types/alert";

export type RuleRow = {
  id: string;
  name: string;
  code: string;
  severity: Severity;
  threshold: number;
  triggeredCount: number;
  enabled: boolean;
  createdAt: string;
};
