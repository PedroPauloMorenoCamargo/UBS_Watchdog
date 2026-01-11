import type { RiskLevelApi } from "../alert";

export type ComplianceRuleResponseDto = {
  id: string;
  code: string;
  ruleType: number;
  name: string;
  isActive: boolean;
  severity: RiskLevelApi;
  scope?: string;
  parameters: Record<string, unknown>;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export interface PagedComplianceRulesResponseDto {
  items: ComplianceRuleResponseDto[];
  page: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}