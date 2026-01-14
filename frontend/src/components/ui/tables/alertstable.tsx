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
import { Eye } from "lucide-react";
import type { CaseTableRow } from "@/models/case";


interface AlertsTableProps {
  alerts: CaseTableRow[];
}

export function AlertsTable({ alerts }: AlertsTableProps){

  return (
    <div className="rounded-lg border bg-white max-h-[420px] overflow-y-auto">
      <Table>
        <TableHeader className="sticky top-0 bg-white z-10">
          <TableRow>
            <TableHead className="px-4 py-3">Alert ID</TableHead>
            <TableHead className="px-4 py-3">Client</TableHead>
            <TableHead className="px-4 py-3">Severity</TableHead>
            <TableHead className="px-4 py-3 text-center">Time</TableHead>
            <TableHead className="px-4 py-3">Status</TableHead>
            <TableHead className="px-4 py-3 text-center">Actions</TableHead>

          </TableRow>
        </TableHeader>

        <TableBody>
          {alerts.length === 0 ? (
            <TableRow>
              <TableCell
                colSpan={8}
                className="h-24 text-center text-sm text-slate-500"
              >
                No records found for selected filters.
              </TableCell>
            </TableRow>
            ) : (
                alerts.map((alert, index) => {
                  return (
                    <TableRow key={`${alert.id}-${index}`}>

                      <TableCell className="px-4 py-3 text-sm">
                        {alert.id}
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm">
                        {alert.clientName}
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm">
                        <SeverityBadge severity={alert.severity} />
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm text-center font-medium">
                          {alert.openedAtUtc}
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm text-muted-foreground">
                        <StatusBadge status={alert.status} />
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm text-left text-slate-600">
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
                  )
                }
              )
            )
          }  
        </TableBody>
      </Table>
    </div>
  );
}

