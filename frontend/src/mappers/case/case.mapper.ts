import type { CaseResponseDto } from "@/types/Cases/cases";
import type { CaseTableRow } from "@/models/case";
import { mapRiskLevel } from "../riskLevel.mapper";
import { mapStatus } from "../alertstatus.mapper";

export function mapCaseDtoToTableRow(dto: CaseResponseDto): CaseTableRow {
  return {
    id: dto.id,
    transactionId: dto.transactionId,
    clientName: dto.clientName,
    accountIdentifier: dto.accountIdentifier,
    status: mapStatus(dto.status),
    decision: dto.decision,
    analystName: dto.analystName,
    severity: mapRiskLevel(dto.severity),
    findingsCount: dto.findingsCount,
    openedAtUtc: new Date(dto.openedAtUtc),
    resolvedAt: dto.resolvedAtUtc ? new Date(dto.resolvedAtUtc).toLocaleString() : null,
  };
}

export function mapPagedCasesDtoToTableRows(apiResponse: {
  items: CaseResponseDto[];
}): CaseTableRow[] {
  return apiResponse.items.map(mapCaseDtoToTableRow);
}
