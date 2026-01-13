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


import { alertsMock } from "@/mocks/mocks";


interface AlertsTableProps {
  alerts: typeof alertsMock;
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
            <TableHead className="px-4 py-3 text-left">Rule Triggered</TableHead>
            <TableHead className="px-4 py-3 text-left">Amount</TableHead>
            <TableHead className="px-4 py-3">Time</TableHead>
            <TableHead className="px-4 py-3">Status</TableHead>
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
                        {alert.client}
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm">
                        <SeverityBadge severity={alert.severity} />
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm">
                        {alert.rule}
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm text-left font-medium">
                        <span
                          className="block max-w-[100px] truncate"
                          title={String(alert.amount)}
                        >
                          {alert.amount}
                        </span>
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm text-muted-foreground">
                        {alert.time}
                      </TableCell>

                      <TableCell className="px-4 py-3 text-sm text-muted-foreground">
                        <StatusBadge status={alert.status} />
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

