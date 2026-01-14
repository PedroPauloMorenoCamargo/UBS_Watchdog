import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { SeverityBadge } from "../severitybadge";
import { StatusBadge } from "../statusbadge";
import { Button } from "../button";
import { Eye, ChevronDown, ChevronUp, AlertCircle } from "lucide-react";
import type { CaseTableRow } from "@/models/case";
import { useState } from "react";
import { getCaseDetails } from "@/services/case.service";
import type { CaseFindingDto } from "@/types/Cases/cases";
import { formatCurrency } from "@/lib/utils";
import { FindingDetailsDialog } from "@/components/ui/dialogs/finding-details-dialog";
import { AlertDetailsDialog } from "@/components/ui/dialogs/alert-details-dialog";

interface AlertsTableProps {
  alerts: CaseTableRow[];
  onRefresh?: () => void;
}

export function AlertsTable({ alerts, onRefresh }: AlertsTableProps){
  const [expandedCaseId, setExpandedCaseId] = useState<string | null>(null);
  const [findings, setFindings] = useState<CaseFindingDto[]>([]);
  const [loadingFindings, setLoadingFindings] = useState(false);
  
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedCaseId, setSelectedCaseId] = useState<string | null>(null);

  const [findingDialogOpen, setFindingDialogOpen] = useState(false);
  const [selectedFinding, setSelectedFinding] = useState<CaseFindingDto | null>(null);

  const toggleExpand = async (caseId: string) => {
    if (expandedCaseId === caseId) {
      setExpandedCaseId(null);
      setFindings([]);
      return;
    }

    setExpandedCaseId(caseId);
    setLoadingFindings(true);
    try {
      const details = await getCaseDetails(caseId);
      setFindings(details.findings);
    } catch (error) {
      console.error("Failed to fetch findings", error);
    } finally {
      setLoadingFindings(false);
    }
  };

  
  const handleViewDetails = (caseId: string) => {
      setSelectedCaseId(caseId);
      setDialogOpen(true);
  };

  const handleViewFinding = (finding: CaseFindingDto) => {
      setSelectedFinding(finding);
      setFindingDialogOpen(true);
  };
  
  const handleDialogUpdate = () => {
      if (onRefresh) {
          onRefresh();
      }
  };

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
    <>
    <div className="rounded-lg border bg-white max-h-[420px] overflow-y-auto">
      <Table>
        <TableHeader className="sticky top-0 bg-white z-10">
          <TableRow>
            <TableHead className="w-[50px]"></TableHead>
            <TableHead className="px-4 py-3">Case ID</TableHead>
            <TableHead className="px-4 py-3">Client</TableHead>
            <TableHead className="px-4 py-3">Alerts Count</TableHead>
            <TableHead className="px-4 py-3">Highest Severity</TableHead>
            <TableHead className="px-4 py-3">Status</TableHead>
            <TableHead className="px-4 py-3">Assigned To</TableHead>
            <TableHead className="px-4 py-3">Created</TableHead>
            <TableHead className="px-4 py-3 text-center">Actions</TableHead>

          </TableRow>
        </TableHeader>

        <TableBody>
          {alerts.length === 0 ? (
            <TableRow>
              <TableCell
                colSpan={10}
                className="h-24 text-center text-sm text-slate-500"
              >
                No records found for selected filters.
              </TableCell>
            </TableRow>
            ) : (
                alerts.map((alert, index) => {
                  return (
                    <>
                    <TableRow key={`${alert.id}-${index}`}>
                      <TableCell className="px-4 py-3 text-sm">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => toggleExpand(alert.id)}
                        >
                          {expandedCaseId === alert.id ? (
                            <ChevronUp className="h-4 w-4" />
                          ) : (
                            <ChevronDown className="h-4 w-4" />
                          )}
                        </Button>
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm font-medium">
                        {alert.id.substring(0, 13)}...
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm">
                        {alert.clientName}
                      </TableCell>
                      
                       <TableCell className="px-4 py-3 text-sm">
                        <div className="flex items-center gap-1 text-slate-500">
                             <AlertCircle className="w-3.5 h-3.5" />
                             {alert.findingsCount}
                        </div>
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm">
                        <SeverityBadge severity={alert.severity} />
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm text-muted-foreground">
                        <StatusBadge status={alert.status} />
                      </TableCell>
                      
                      <TableCell className="px-4 py-3 text-sm text-slate-500">
                        {alert.analystName || "-"}
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm text-left text-slate-500 whitespace-nowrap">
                          {new Date(alert.openedAtUtc).toLocaleString('sv').slice(0, 16).replace('T', ' ')}
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm text-left text-slate-600">
                      <Button
                          variant="ghost"
                          size="sm"
                          className="text-[#e60028] hover:text-[#b8001f] hover:bg-red-50"
                          onClick={() => handleViewDetails(alert.id)}
                        >
                          <Eye className="w-4 h-4 mr-1" />
                          View
                        </Button>
                    </TableCell>
                    </TableRow>
                    {expandedCaseId === alert.id && (
                      <TableRow>
                        <TableCell colSpan={10} className="bg-slate-50 p-4">
                          <div className="rounded-md border bg-white p-4">
                            <h4 className="mb-4 text-sm font-semibold text-slate-900">
                              Alerts Details
                            </h4>
                            {loadingFindings ? (
                              <p className="text-sm text-slate-500">Loading details...</p>
                            ) : findings.length > 0 ? (
                              <Table>
                                <TableHeader>
                                  <TableRow>
                                    <TableHead>Alert ID</TableHead>
                                    <TableHead>Rule</TableHead>
                                    <TableHead className="w-[40%]">Description</TableHead>
                                    <TableHead>Severity</TableHead>
                                    <TableHead>Amount</TableHead>
                                    <TableHead>Created</TableHead>
                                    <TableHead></TableHead>
                                  </TableRow>
                                </TableHeader>
                                <TableBody>
                                  {findings.map((f) => (
                                    <TableRow key={f.id}>
                                      <TableCell className="font-medium text-xs text-slate-500">
                                        {f.id.substring(0, 12)}...
                                      </TableCell>
                                      <TableCell>{f.ruleName}</TableCell>
                                      <TableCell className="text-xs text-slate-600">{f.evidenceJson.message}</TableCell>
                                      <TableCell>
                                        <SeverityBadge severity={getSeverityString(f.severity) as any} />
                                      </TableCell>
                                      <TableCell>
                                        {formatCurrency(f.evidenceJson.amount || 0, 'USD')}
                                      </TableCell>
                                       <TableCell className="text-xs text-slate-500">
                                        {new Date(f.createdAtUtc).toLocaleString()}
                                      </TableCell>
                                      <TableCell>
                                         <Button 
                                            variant="ghost" 
                                            size="sm" 
                                            className="text-red-600 h-6"
                                            onClick={() => handleViewFinding(f)}
                                         >
                                            <Eye className="w-3 h-3 mr-1" /> Review
                                         </Button>
                                      </TableCell>
                                    </TableRow>
                                  ))}
                                </TableBody>
                              </Table>
                            ) : (
                              <p className="text-sm text-slate-500">No findings available.</p>
                            )}
                          </div>
                        </TableCell>
                      </TableRow>
                    )}
                    </>
                  )
                })
            )
          }  
        </TableBody>
      </Table>
    </div>
    
    <AlertDetailsDialog 
        open={dialogOpen} 
        onOpenChange={setDialogOpen} 
        caseId={selectedCaseId} 
        onUpdate={handleDialogUpdate}
    />
    <FindingDetailsDialog
        open={findingDialogOpen}
        onOpenChange={setFindingDialogOpen}
        finding={selectedFinding}
    />
    </>
  );
}
