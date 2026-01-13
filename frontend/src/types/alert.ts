export type Severity ="Critical" | "critical"| "High" | "Medium" | "Low" | "high" | "medium" | "low";
export type SeverityFilter = Severity | "all";
export type RiskLevelApi = 0 | 1 | 2;

export interface AlertItem {
  id: string;
  title: string;
  description: string;
  severity: SeverityFilter;
  date: string;
}

