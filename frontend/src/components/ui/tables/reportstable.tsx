import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

import { Button } from "../button";
import { Eye } from "lucide-react";
import type { ClientReportDto } from "@/types/Reports/report";
import { useRef } from "react";

interface ReportsTableProps {
  reports: ClientReportDto[];
}

export function ReportsTable({ reports }: ReportsTableProps) {
  const tableRef = useRef<HTMLDivElement | null>(null);
  console.log("Cade os usuarios",reports)
  return (
    <div
      ref={tableRef}
      className="rounded-lg border bg-white max-h-[420px] overflow-y-auto"
    >
      <Table className="w-full">
        <TableHeader className="sticky top-0 z-10 bg-white">
          <TableRow>
            <TableHead className="px-4 py-3">Client Name</TableHead>
            <TableHead className="px-4 py-3">Alerts</TableHead>
            <TableHead className="px-4 py-3">Balance (USD)</TableHead>
            <TableHead className="px-4 py-3 text-center">Risk Level</TableHead>
            <TableHead className="px-4 py-3 text-center">Actions</TableHead>
          </TableRow>
        </TableHeader>

        <TableBody>
          {reports.length === 0 ? (
            <TableRow>
              <TableCell colSpan={5} className="h-24 text-center text-sm text-slate-500">
                No reports found.
              </TableCell>
            </TableRow>
          ) : (
            reports.map((report) => (
              <TableRow key={report.clientId}>
                <TableCell className="px-4 py-3 text-sm font-medium text-slate-700">
                  {report.clientName}
                </TableCell>

                {/* Número de alertas */}
                <TableCell className="px-4 py-3 text-sm text-left text-slate-600">
                  {report.caseMetrics.totalCases}
                </TableCell>

                {/* Balance total */}
                <TableCell className="px-4 py-3 text-sm text-left text-slate-600">
                  ${report.transactionMetrics.totalVolumeUSD.toLocaleString()}
                </TableCell>

                {/* Risk Level */}
                <TableCell className="px-4 py-3 text-sm text-center text-slate-600">
                  {report.riskLevel}
                </TableCell>

                {/* Actions */}
                <TableCell className="px-4 py-3 text-sm text-center text-slate-600">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-[#e60028] hover:text-[#b8001f] hover:bg-red-50"
                  >
                    <Eye className="w-4 h-4 mr-1" />
                    View
                  </Button>
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  );
}
