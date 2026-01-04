import { useMemo } from "react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { SeverityBadge } from "../severitybadge";


import { alertsMock } from "@/mocks/mocks";

type SeverityFilter = "all" | "high" | "medium" | "low";

interface AlertsTableProps {
  severityFilter: SeverityFilter;
}

export function AlertsTable({ severityFilter }: AlertsTableProps){

    const filteredAlerts = useMemo(() => {
    if (severityFilter === "all") return alertsMock;

    return alertsMock.filter(
      (alert) =>
        alert.severity.toLowerCase() === severityFilter
    );
  }, [severityFilter]);

  return (
    <div className="rounded-lg border bg-white max-h-[420px] overflow-y-auto">
      <Table>
        <TableHeader className="sticky top-0 bg-white z-10">
          <TableRow>
            <TableHead className="w-[150px]">Alert ID</TableHead>
            <TableHead className="w-[200px]">Client</TableHead>
            <TableHead className="w-[120px]">Severity</TableHead>
            <TableHead>Rule Triggered</TableHead>
            <TableHead className="w-[120px] text-left">Amount</TableHead>
            <TableHead className="w-[110px]">Time</TableHead>
          </TableRow>
        </TableHeader>

        <TableBody>
          {filteredAlerts.map((alert, index) => (
            <TableRow key={`${alert.id}-${index}`}>
              <TableCell className="w-[150px] font-medium">
                {alert.id}
              </TableCell>

              <TableCell className="w-[200px]">
                {alert.client}
              </TableCell>

              <TableCell className="w-[120px]">
                <SeverityBadge severity={alert.severity} />
              </TableCell>

              <TableCell>
                {alert.rule}
              </TableCell>

              <TableCell className="w-[120px] text-left font-medium">
                <span
                  className="block max-w-[120px] truncate"
                  title={String(alert.amount)}
                >
                  {alert.amount}
                </span>
              </TableCell>

              <TableCell className="w-[110px] text-muted-foreground">
                {alert.time}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

