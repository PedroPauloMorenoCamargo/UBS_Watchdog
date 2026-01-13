// mappers/case.mapper.ts
import type { CaseResponseDto } from "@/types/Cases/cases";
import type { CaseTableRow } from "@/models/case";

export function mapCaseDtoToTableRow(dto: CaseResponseDto): CaseTableRow {
  return {
    id: dto.id,
    transactionId: dto.transactionId,
    clientName: dto.clientName,
    accountIdentifier: dto.accountIdentifier,
    status: dto.status,
    decision: dto.decision,
    analystName: dto.analystName,
    severity: dto.severity,
    findingsCount: dto.findingsCount,
    openedAt: new Date(dto.openedAt).toLocaleString(),
    resolvedAt: dto.resolvedAt ? new Date(dto.resolvedAt).toLocaleString() : null,
  };
}

export function mapPagedCasesDtoToTableRows(apiResponse: {
  items: CaseResponseDto[];
}): CaseTableRow[] {
  return apiResponse.items.map(mapCaseDtoToTableRow);
}
