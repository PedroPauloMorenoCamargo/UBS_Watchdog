import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import type { AuditLogDto } from "@/types/audit-log";
import { AuditAction } from "@/types/audit-log";

interface AuditLogDetailsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  log: AuditLogDto | null;
  analystName?: string;
}

export function AuditLogDetailsDialog({
  open,
  onOpenChange,
  log,
  analystName,
}: AuditLogDetailsDialogProps) {
  if (!log) return null;

  const getActionColor = (action: AuditAction) => {
    switch (action) {
      case AuditAction.Created:
        return "bg-blue-100 text-blue-800 hover:bg-blue-200";
      case AuditAction.Updated:
        return "bg-yellow-100 text-yellow-800 hover:bg-yellow-200";
      case AuditAction.Deleted:
        return "bg-red-100 text-red-800 hover:bg-red-200";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const getActionLabel = (action: AuditAction) => {
    switch (action) {
      case AuditAction.Created:
        return "Created";
      case AuditAction.Updated:
        return "Updated";
      case AuditAction.Deleted:
        return "Deleted";
      default:
        return "Unknown";
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh]">
        <DialogHeader>
          <div className="flex items-center gap-3 mb-2">
            <DialogTitle>Audit Log Details</DialogTitle>
            <Badge className={getActionColor(log.action)} variant="secondary">
              {getActionLabel(log.action)}
            </Badge>
          </div>
          <DialogDescription>
            ID: <span className="font-mono text-xs">{log.id}</span>
          </DialogDescription>
        </DialogHeader>

        <div className="h-full max-h-[600px] overflow-y-auto pr-4">
          <div className="grid grid-cols-2 gap-4 mb-6 text-sm">
            <div>
              <p className="text-muted-foreground mb-1">Entity</p>
              <div className="font-medium flex items-center gap-2">
                <Badge variant="outline">{log.entityType}</Badge>
                <span className="font-mono text-xs text-muted-foreground">
                  {log.entityId}
                </span>
              </div>
            </div>
            <div>
              <p className="text-muted-foreground mb-1">Performed By</p>
              <p className="font-medium">{analystName || log.performedByAnalystId || "System"}</p>
            </div>
            <div>
              <p className="text-muted-foreground mb-1">Timestamp</p>
              <p className="font-medium">
                {new Date(log.performedAtUtc).toLocaleString()}
              </p>
            </div>
            <div>
              <p className="text-muted-foreground mb-1">Correlation ID</p>
              <p className="font-mono text-xs">{log.correlationId}</p>
            </div>
          </div>

          <div className="space-y-4">
            {log.before && (
              <div>
                <h4 className="text-sm font-semibold mb-2 text-muted-foreground">Before</h4>
                <div className="bg-slate-50 p-3 rounded-md border text-xs font-mono overflow-auto">
                  <pre>{JSON.stringify(log.before, null, 2)}</pre>
                </div>
              </div>
            )}

            {log.after && (
              <div>
                <h4 className="text-sm font-semibold mb-2 text-muted-foreground">
                  {log.action === AuditAction.Created ? "Data" : "After"}
                </h4>
                <div className="bg-slate-50 p-3 rounded-md border text-xs font-mono overflow-auto">
                  <pre>{JSON.stringify(log.after, null, 2)}</pre>
                </div>
              </div>
            )}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
