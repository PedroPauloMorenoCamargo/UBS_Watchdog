import type { Severity } from "@/types/alert";

const RISK_LEVEL_MAP: Record<0 | 1 | 2 | 3, Severity> = {
  0: "low",
  1: "medium",
  2: "high",
  3: "critical",
};

export function mapRiskLevel(
  risk: 0 | 1 | 2 | 3
): Severity {
  return RISK_LEVEL_MAP[risk];
}