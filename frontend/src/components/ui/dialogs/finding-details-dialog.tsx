import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import type { CaseFindingDto } from "@/types/Cases/cases";
import { formatCurrency } from "@/lib/utils";
import { SeverityBadge } from "@/components/ui/severitybadge";


interface FindingDetailsDialogProps {
  finding: CaseFindingDto | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  currencyCode?: string;
}

export function FindingDetailsDialog({
  finding,
  open,
  onOpenChange,
  currencyCode = "USD",
}: FindingDetailsDialogProps) {
  if (!finding) return null;

  const { evidenceJson } = finding;

  // severity string helper (duplication from table, could be util but keeping simple)
  const getSeverityString = (severity: number) => {
    switch (severity) {
      case 0: return "Low";
      case 1: return "Medium";
      case 2: return "High";
      case 3: return "Critical";
      default: return "Low";
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            Finding Details
            <span className="text-sm font-normal text-muted-foreground ml-2">
              {finding.id.split('-')[0]}...
            </span>
          </DialogTitle>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <div className="text-xs font-semibold uppercase text-muted-foreground mb-1">
                Rule Name
              </div>
              <div className="text-sm font-medium">{finding.ruleName}</div>
            </div>
            <div>
              <div className="text-xs font-semibold uppercase text-muted-foreground mb-1">
                Severity
              </div>
              <div>
                <SeverityBadge severity={getSeverityString(finding.severity)} />
              </div>
            </div>
            <div>
              <div className="text-xs font-semibold uppercase text-muted-foreground mb-1">
                Amount
              </div>
              <div className="text-sm font-medium text-foreground">
                {formatCurrency(evidenceJson.amount || 0, currencyCode)}
              </div>
            </div>
            <div>
              <div className="text-xs font-semibold uppercase text-muted-foreground mb-1">
                Created At
              </div>
              <div className="text-sm text-foreground">
                {new Date(finding.createdAtUtc).toLocaleString()}
              </div>
            </div>
          </div>

          <div>
             <div className="text-xs font-semibold uppercase text-muted-foreground mb-2">
                Description
              </div>
              <div className="rounded-md border bg-muted/30 p-3 text-sm">
                {evidenceJson.message || "No description provided."}
              </div>
          </div>
          
           <div>
             <div className="text-xs font-semibold uppercase text-muted-foreground mb-2">
                Full Evidence
              </div>
              <div className="h-[200px] w-full rounded-md border p-4 bg-slate-50 overflow-y-auto">
                <pre className="text-xs font-mono whitespace-pre-wrap text-slate-700">
                  {JSON.stringify(evidenceJson, null, 2)}
                </pre>
              </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
