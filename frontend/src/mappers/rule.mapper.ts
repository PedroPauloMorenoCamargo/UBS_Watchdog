import type { RuleRow } from "@/models/rule";
import { mapRiskLevel } from "./riskLevel.mapper";
import type { ComplianceRuleResponseDto } from "@/types/Rules/rule";


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