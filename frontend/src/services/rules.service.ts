import { api } from "@/lib/api";
import type { PagedComplianceRulesResponseDto, ComplianceRuleResponseDto } from "@/types/Rules/rule";
import type { PatchRuleRequest } from "@/types/rules";

export async function fetchRules(): Promise<PagedComplianceRulesResponseDto> {
  const response = await api.get("api/rules");
  return response.data;
}

export async function fetchRuleById(id: string): Promise<ComplianceRuleResponseDto> {
  const response = await api.get(`api/rules/${id}`);
  return response.data;
}

export async function patchRule(id: string, data: PatchRuleRequest): Promise<ComplianceRuleResponseDto> {
  const response = await api.patch(`api/rules/${id}`, data);
  return response.data;
}

export async function toggleRuleActive(id: string, isActive: boolean): Promise<ComplianceRuleResponseDto> {
  const response = await api.patch(`api/rules/${id}`, { isActive });
  return response.data;
}
