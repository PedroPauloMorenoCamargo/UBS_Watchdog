import type { RuleRow } from "@/models/rule";
import { mapRiskLevel } from "../riskLevel.mapper";
import type { ComplianceRuleResponseDto } from "@/types/Rules/rule";
import type { Rule } from "@/types/rules";

const RULE_DESCRIPTIONS: Record<string, string> = {
  "daily_limit": "Daily transaction limit in USD per client",
  "banned_countries": "Restricted countries for transactions",
  "banned_accounts": "Blocked accounts for transactions",
  "structuring": "Detection of transaction structuring patterns",
};

export function mapRuleToCard(dto: ComplianceRuleResponseDto): RuleRow {
  return {
    id: dto.id,
    code: dto.code,
    name: dto.name,
    severity: mapRiskLevel(dto.severity),
    threshold: 0,
    triggeredCount: 0, 
    enabled: dto.isActive,
    createdAt: dto.createdAtUtc
  };
}

export function mapDtoToRule(dto: ComplianceRuleResponseDto): Rule {
  const severity = mapRiskLevel(dto.severity);
  return {
    id: dto.id,
    code: dto.code,
    name: dto.name,
    ruleType: dto.ruleType,
    severity,
    threshold: getThresholdFromParams(dto.parameters),
    triggeredCount: 0,
    enabled: dto.isActive,
    scope: dto.scope,
    parameters: dto.parameters,
    description: RULE_DESCRIPTIONS[dto.code] ?? `Compliance rules: ${dto.name}`,
    createdAt: dto.createdAtUtc,
    updatedAt: dto.updatedAtUtc,
  };
}

function getThresholdFromParams(params: Record<string, unknown>): number {
  if (params.limitBaseAmount !== undefined) {
    return Number(params.limitBaseAmount);
  }
  if (params.maxTransactions !== undefined) {
    return Number(params.maxTransactions);
  }
  return 0;
}