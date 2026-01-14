import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { AuditLogDto } from "@/types/audit-log";
import { AuditAction } from "@/types/audit-log";

interface AuditLogTableProps {
  logs: AuditLogDto[];
  userMap?: Record<string, string>;
  onRowClick?: (log: AuditLogDto) => void;
}

export function AuditLogTable({ logs, userMap = {}, onRowClick }: AuditLogTableProps) {
  const getActionBadge = (action: AuditAction) => {
    switch (action) {
      case AuditAction.Created:
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
            Created
          </span>
        );
      case AuditAction.Updated:
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
            Updated
          </span>
        );
      case AuditAction.Deleted:
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
            Deleted
          </span>
        );
      default:
        return (
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
            Unknown
          </span>
        );
    }
  };

  const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp);
    return date.toLocaleString("en-US", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
      hour12: false,
    });
  };

  const getChangeSummary = (log: AuditLogDto) => {
    if (log.action === AuditAction.Created && log.after) {
      return `Created ${log.entityType.toLowerCase()} with ID ${log.entityId}`;
    }
    
    if (log.action === AuditAction.Updated && log.before && log.after) {
      const changedFields = Object.keys(log.after).filter(
        key => JSON.stringify(log.before?.[key]) !== JSON.stringify(log.after?.[key])
      );
      
      if (changedFields.length > 0) {
        return `Updated ${changedFields.join(", ")} (ID: ${log.entityId})`;
      }
      return `Updated entity (ID: ${log.entityId})`;
    }
    
    if (log.action === AuditAction.Deleted) {
      return `Deleted ${log.entityType.toLowerCase()} (ID: ${log.entityId})`;
    }
    
    return "Action performed";
  };

  if (logs.length === 0) {
    return (
      <div className="text-center py-8 text-gray-500">
        No audit logs found
      </div>
    );
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-[180px]">Timestamp</TableHead>
            <TableHead className="w-[150px]">User</TableHead>
            <TableHead className="w-[120px]">Action</TableHead>
            <TableHead className="w-[120px]">Entity Type</TableHead>
            <TableHead>Details</TableHead>
            <TableHead className="w-[150px]">Correlation ID</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {logs.map((log) => (
            <TableRow 
              key={log.id} 
              className="cursor-pointer hover:bg-slate-50 transition-colors"
              onClick={() => onRowClick?.(log)}
            >
              <TableCell className="font-mono text-xs">
                {formatTimestamp(log.performedAtUtc)}
              </TableCell>
              <TableCell className="text-sm">
                <span className={log.performedByAnalystId && userMap[log.performedByAnalystId] ? "text-gray-900" : "font-mono text-xs text-gray-700"}>
                  {log.performedByAnalystId ? (userMap[log.performedByAnalystId] || log.performedByAnalystId) : "System"}
                </span>
              </TableCell>
              <TableCell>{getActionBadge(log.action)}</TableCell>
              <TableCell>
                <span className="inline-flex items-center px-2 py-1 rounded text-xs font-medium bg-slate-100 text-slate-700">
                  {log.entityType}
                </span>
              </TableCell>
              <TableCell className="text-sm text-gray-600">
                {getChangeSummary(log)}
              </TableCell>
              <TableCell className="font-mono text-xs text-gray-500">
                {log.correlationId}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
