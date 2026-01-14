export const AuditAction = {
  Created: 0,
  Updated: 1,
  Deleted: 2,
} as const;

export type AuditAction = typeof AuditAction[keyof typeof AuditAction];

export type EntityType = "Case" | "Transaction" | "Client" | "Account" | "Rule" | "User";

export interface AuditLogDto {
  id: string;
  entityType: EntityType;
  entityId: string;
  action: AuditAction;
  performedByAnalystId: string;
  correlationId: string;
  before: Record<string, unknown> | null;
  after: Record<string, unknown> | null;
  performedAtUtc: string;
}

export interface PagedAuditLogsResponseDto {
  items: AuditLogDto[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
