import { api } from "@/lib/api";
import type { PagedComplianceRulesResponseDto } from "@/types/Rules/rule";

export async function fetchRules(): Promise<PagedComplianceRulesResponseDto> {
  const response = await api.get("api/rules");
  return response.data;
}
