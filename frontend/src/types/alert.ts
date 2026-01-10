export type Severity = "High" | "Medium" | "Low";
export type SeverityFilter = Severity | "all";

export interface AlertItem {
  id: string;
  title: string;
  description: string;
  severity: Severity;
  date: string;
}

