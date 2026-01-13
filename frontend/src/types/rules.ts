import type { Severity } from "./alert";

export type Rule = {
  id: string;
  code: string;
  name: string;
  ruleType: number;
  severity: Severity;
  threshold: number;
  triggeredCount: number;
  enabled: boolean;
  scope?: string;
  parameters: Record<string, unknown>;
  description?: string;
  createdAt: string;
  updatedAt: string;
};

export type PatchRuleRequest = {
  name?: string;
  isActive?: boolean;
  severity?: "Low" | "Medium" | "High" | "Critical";
  scope?: string;
  parameters?: Record<string, unknown>;
};
