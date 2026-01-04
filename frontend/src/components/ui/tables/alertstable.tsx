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
    <div className="rounded-lg border bg-white">
      
      {/* HEADER */}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Alert ID</TableHead>
            <TableHead>Client</TableHead>
            <TableHead>Severity</TableHead>
            <TableHead>Rule Triggered</TableHead>
            <TableHead className="text-right">Amount</TableHead>
            <TableHead>Time</TableHead>
          </TableRow>
        </TableHeader>
      </Table>

      {/* BODY COM SCROLL */}
      <div className="max-h-[420px] overflow-y-auto">
        <Table>
          <TableBody>
            {filteredAlerts.map((alert, index) => (
              <TableRow key={`${alert.id}-${index}`}>
                <TableCell className="font-medium">
                  {alert.id}
                </TableCell>
                <TableCell>{alert.client}</TableCell>
                <TableCell>
                  <SeverityBadge severity={alert.severity} />
                </TableCell>
                <TableCell>{alert.rule}</TableCell>
                <TableCell className="text-right font-semibold">
                  {alert.amount}
                </TableCell>
                <TableCell className="text-muted-foreground">
                  {alert.time}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}

