import type { Severity } from "@/types/alert";

export function mapRiskLevel(
  risk: 0 | 1 | 2
): Severity {
  switch (risk) {
    case 0:
      return "low";
    case 1:
      return "medium";
    case 2:
      return "high";
  }
}
