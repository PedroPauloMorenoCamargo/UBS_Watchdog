import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { CaseStatus, CaseDecision } from "@/types/Cases/cases";
import type { CaseDetailedResponseDto, CaseFindingDto } from "@/types/Cases/cases";
import { getCaseDetails, updateCase, assignToMe } from "@/services/case.service";
import { SeverityBadge } from "@/components/ui/severitybadge";
import { formatCurrency } from "@/lib/utils";
import { CheckCircle2, UserPlus, AlertTriangle } from "lucide-react";
import { api } from "@/lib/api";

interface AlertDetailsDialogProps {
  caseId: string | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onUpdate: () => void;
}

export function AlertDetailsDialog({ 
  caseId, 
  open, 
  onOpenChange,
  onUpdate
}: AlertDetailsDialogProps) {
  const [data, setData] = useState<CaseDetailedResponseDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [updating, setUpdating] = useState(false);
  const [currentAnalystId, setCurrentAnalystId] = useState<string | null>(null);

  useEffect(() => {
    // Fetch current user ID for resolving cases
    api.get("/api/auth/me").then(res => {
         // Assuming response has id or similar. Adjust based on actual DTO if known.
         // Common patterns: res.data.id, res.data.sub, etc. 
         // User provided example used a guid.
         if (res.data?.id) setCurrentAnalystId(res.data.id);
    }).catch(err => console.error("Failed to fetch current user", err));
  }, []);

  useEffect(() => {
    if (open && caseId) {
      loadCaseDetails(caseId);
    }
  }, [open, caseId]);

  const loadCaseDetails = async (id: string) => {
    try {
      setLoading(true);
      const details = await getCaseDetails(id);
      setData(details);
    } catch (error) {
      console.error("Failed to load case details", error);
    } finally {
      setLoading(false);
    }
  };

  const handleAssignToMe = async () => {
    if (!data) return;
    try {
      setUpdating(true);
      await assignToMe(data.id);
      await loadCaseDetails(data.id);
      onUpdate();
    } catch (error) {
      console.error("Failed to assign case", error);
    } finally {
      setUpdating(false);
    }
  };

  const handleUpdateStatus = async (status: number, decision?: number) => {
    if (!data) return;
    try {
      setUpdating(true);
      await updateCase(data.id, { 
          status: status as any, 
          decision: decision !== undefined ? (decision as any) : undefined,
          analystId: currentAnalystId || undefined 
      });
      await loadCaseDetails(data.id);
      onUpdate();
    } catch (error) {
      console.error("Failed to update status", error);
    } finally {
      setUpdating(false);
    }
  };

  if (!open) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl max-h-[90vh] flex flex-col p-0 gap-0">
        {loading || !data ? (
           <div className="p-8 text-center">Loading details...</div>
        ) : (
          <>
            <div className="p-6 border-b">
              <DialogHeader>
                <DialogTitle className="text-xl flex items-center gap-2">
                  Alert Details - {data.id.substring(0, 8)} 
                </DialogTitle>
                <div className="text-sm text-muted-foreground mt-1">
                    {data.clientName}
                </div>
              </DialogHeader>

              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-6">
                 <div>
                    <div className="text-xs text-muted-foreground uppercase tracking-wider font-semibold">Case ID</div>
                    <div className="font-medium text-sm mt-1">{data.id.split('-')[0]}...</div>
                 </div>
                 <div>
                    <div className="text-xs text-muted-foreground uppercase tracking-wider font-semibold">Transaction ID</div>
                    <div className="font-medium text-sm mt-1">{data.transactionId.split('-')[0]}...</div>
                 </div>
                 <div>
                    <div className="text-xs text-muted-foreground uppercase tracking-wider font-semibold">Amount</div>
                    <div className="font-medium text-sm mt-1">{formatCurrency(data.totalAmount || 0, data.currencyCode)}</div>
                 </div>
                 <div>
                    <div className="text-xs text-muted-foreground uppercase tracking-wider font-semibold">Severity</div>
                    <div className="mt-1"><SeverityBadge severity={getSeverityString(data.severity)} /></div>
                 </div>
                 
                 <div>
                    <div className="text-xs text-muted-foreground uppercase tracking-wider font-semibold">Rule Triggered</div>
                    <div className="font-medium text-sm mt-1 truncate" title={data.findings[0]?.ruleName}>
                      {data.findings[0]?.ruleName || "Multiple Rules"}
                    </div>
                 </div>
                 <div>
                    <div className="text-xs text-muted-foreground uppercase tracking-wider font-semibold">Timestamp</div>
                    <div className="font-medium text-sm mt-1">{new Date(data.openedAtUtc).toLocaleString()}</div>
                 </div>
                 <div>
                    <div className="text-xs text-muted-foreground uppercase tracking-wider font-semibold">Assigned To</div>
                    <div className="font-medium text-sm mt-1 flex items-center gap-2">
                        {data.analystName ? (
                            <span>{data.analystName}</span>
                        ) : (
                            <Button variant="outline" size="sm" onClick={handleAssignToMe} disabled={updating} className="h-6 text-xs px-2">
                                <UserPlus className="w-3 h-3 mr-1" /> Assign Me
                            </Button>
                        )}
                    </div>
                 </div>
              </div>

              {data.findings.length > 0 && (
                  <div className="mt-6">
                    <div className="text-xs text-muted-foreground uppercase tracking-wider font-semibold mb-2">Description</div>
                    <div className="bg-muted/30 p-3 rounded-md text-sm border">
                        {data.findings[0]?.evidenceJson?.message || "No description available."}
                    </div>
                  </div>
              )}
            </div>

            <div className="flex-1 p-6 overflow-y-auto">
                <h3 className="text-sm font-semibold mb-4 text-muted-foreground">Related Alerts in This Case</h3>
                <div className="space-y-3">
                    {data.findings.map((finding: CaseFindingDto) => (
                        <div key={finding.id} className="flex items-center justify-between p-3 border rounded-lg bg-card/50 hover:bg-muted/50 transition-colors">
                            <div className="flex items-center gap-4">
                                <span className="text-xs font-mono text-muted-foreground">{finding.id.split('-')[0]}</span>
                                <SeverityBadge severity={getSeverityString(finding.severity)} />
                                <span className="text-sm font-medium">{finding.ruleName}</span>
                            </div>
                            <div className="text-sm font-medium">
                                {formatCurrency(finding.evidenceJson?.amount || 0, data.currencyCode)}
                            </div>
                        </div>
                    ))}
                </div>
            </div>

            <div className="p-6 border-t bg-muted/10">
                <div className="mb-4">
                    <div className="text-xs font-semibold uppercase text-muted-foreground mb-3">Update Status</div>
                    <div className="flex flex-wrap gap-2 mb-4">
                        <Button 
                            variant={data.status === CaseStatus.New ? "default" : "outline"}
                            size="sm"
                            onClick={() => handleUpdateStatus(CaseStatus.New)}
                            disabled={data.status === CaseStatus.New || updating || data.status === CaseStatus.Resolved}
                            className={data.status === CaseStatus.New ? "bg-blue-600 hover:bg-blue-700" : ""}
                        >
                            New
                        </Button>
                        <Button 
                            variant={data.status === CaseStatus.UnderReview ? "default" : "outline"} 
                            size="sm"
                            onClick={() => handleUpdateStatus(CaseStatus.UnderReview)}
                            disabled={data.status === CaseStatus.UnderReview || updating}
                             className={data.status === CaseStatus.UnderReview ? "bg-amber-600 hover:bg-amber-700" : ""}
                        >
                            In Analysis
                        </Button>
                        <div className="h-8 w-px bg-border mx-2" />
                        <Button 
                             variant={data.status === CaseStatus.Resolved && data.decision === undefined ? "default" : "outline"}
                             size="sm"
                             onClick={() => handleUpdateStatus(CaseStatus.Resolved)}
                             disabled={data.status === CaseStatus.Resolved || updating}
                             className={data.status === CaseStatus.Resolved && data.decision === undefined ? "bg-slate-600 hover:bg-slate-700" : ""}
                        >
                            <CheckCircle2 className="w-4 h-4 mr-1.5" />
                            Resolved
                        </Button>
                    </div>

                    <div className="text-xs font-semibold uppercase text-muted-foreground mb-3">Resolution & Decision</div>
                    <div className="flex flex-wrap gap-2">
                         <Button 
                            variant={data.status === CaseStatus.Resolved && data.decision === CaseDecision.Fraudulent ? "default" : "outline"} 
                            size="sm"
                            onClick={() => handleUpdateStatus(CaseStatus.Resolved, CaseDecision.Fraudulent)}
                            disabled={updating}
                             className={data.status === CaseStatus.Resolved && data.decision === CaseDecision.Fraudulent ? "bg-red-600 hover:bg-red-700" : ""}
                        >
                            <AlertTriangle className="w-4 h-4 mr-1.5" />
                            Fraudulent
                        </Button>

                        <Button 
                            variant={data.status === CaseStatus.Resolved && data.decision === CaseDecision.NotFraudulent ? "default" : "outline"} 
                            size="sm"
                            onClick={() => handleUpdateStatus(CaseStatus.Resolved, CaseDecision.NotFraudulent)}
                            disabled={updating}
                            className={data.status === CaseStatus.Resolved && data.decision === CaseDecision.NotFraudulent ? "bg-green-600 hover:bg-green-700" : ""}
                        >
                            <CheckCircle2 className="w-4 h-4 mr-1.5" />
                            Not Fraudulent
                        </Button>

                         <Button 
                            variant={data.status === CaseStatus.Resolved && data.decision === CaseDecision.Inconclusive ? "default" : "outline"} 
                            size="sm"
                            onClick={() => handleUpdateStatus(CaseStatus.Resolved, CaseDecision.Inconclusive)}
                            disabled={updating}
                             className={data.status === CaseStatus.Resolved && data.decision === CaseDecision.Inconclusive ? "bg-slate-600 hover:bg-slate-700" : ""}
                        >
                            <AlertTriangle className="w-4 h-4 mr-1.5" />
                            Inconclusive
                        </Button>

                    </div>
                </div>
            </div>
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}

function getSeverityString(severity: number) {
    switch (severity) {
      case 0: return "Low";
      case 1: return "Medium";
      case 2: return "High";
      case 3: return "Critical";
      default: return "Low";
    }
}
