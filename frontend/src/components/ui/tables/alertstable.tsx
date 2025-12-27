
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

export function AlertsTable() {
  return (
    <div className="overflow-x-auto">
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

        <TableBody>
          {alertsMock.map((alert) => (
            <TableRow key={alert.id}>
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
  );
}
