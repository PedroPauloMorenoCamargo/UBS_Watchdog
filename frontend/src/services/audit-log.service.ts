import { api } from "@/lib/api";
import type { PagedAuditLogsResponseDto, AuditLogDto, AuditAction } from "@/types/audit-log";

export interface FetchAuditLogsParams {
  page?: number;
  pageSize?: number;
  action?: AuditAction;
}

export function fetchAuditLogs(params?: FetchAuditLogsParams) {
  return api
    .get<PagedAuditLogsResponseDto>("api/audit-logs", {
      params: {
        page: params?.page ?? 1,
        pageSize: params?.pageSize ?? 50,
        action: params?.action,
      },
    })
    .then((res) => res.data);
}

export function getAuditLogDetails(id: string) {
  return api.get<AuditLogDto>(`api/audit-logs/${id}`).then((res) => res.data);
}
