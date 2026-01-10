import type { Severity } from "./alert";

export type Rule = {
  id: string;
  name: string;
  description: string;
  severity: Severity;
  threshold: number;
  triggeredCount: number;
  enabled: boolean;
};
