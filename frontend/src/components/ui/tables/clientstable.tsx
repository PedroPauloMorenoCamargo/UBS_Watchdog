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
import { SeverityBadge } from "../severitybadge";
import { useRef } from "react";
import { KYCStatusBadge } from "../kycstatusbadge";
import type{ ClientTableRow } from "@/models/client";
import { formatDateTime } from "@/lib/utils";

interface ClientsTableProps {
  clients: ClientTableRow[];
}

export function ClientsTable({
  clients,
}: ClientsTableProps) {
  const tableRef = useRef<HTMLDivElement | null>(null);

  return (
    <div
      ref={tableRef}
      className="rounded-lg border bg-white max-h-[420px] overflow-y-auto"
    >
      <Table className="w-full">
        <TableHeader className="sticky top-0 z-10 bg-white">
          <TableRow>
            <TableHead className="px-4 py-3">Client ID</TableHead>
            <TableHead className="px-4 py-3">Name</TableHead>
            <TableHead className="px-4 py-3">Country</TableHead>
            <TableHead className="px-4 py-3 text-center">Risk Level</TableHead>
            <TableHead className="px-4 py-3 text-center">KYC Status</TableHead>
            <TableHead className="px-4 py-3 text-center">Alerts</TableHead>
            <TableHead className="px-4 py-3 text-left">Balance</TableHead>
            <TableHead className="px-4 py-3 text-left">Last Activity</TableHead>
            <TableHead className="px-4 py-3 text-center">Actions</TableHead>
          </TableRow>
        </TableHeader>

        <TableBody>
          {clients.length === 0 ? (
            <TableRow>
              <TableCell
                colSpan={8}
                className="h-24 text-center text-sm text-slate-500"
              >
                No records found for selected filters.
              </TableCell>
            </TableRow>
            ) : (
              clients.map((client) => {
                return (
                  <TableRow>
                    <TableCell className="px-4 py-3 text-sm font-medium text-slate-700">
                      {client.id}
                    </TableCell>

                    <TableCell className="px-4 py-3 text-sm font-medium text-slate-700">
                      {client.name}
                    </TableCell>

                    <TableCell className="px-4 py-3 text-sm text-slate-600">
                      {client.country}
                    </TableCell>

                    <TableCell className="px-4 py-3 text-sm text-center">
                      <SeverityBadge severity={client.risk} />
                    </TableCell>

                    <TableCell className="px-4 py-3 text-sm text-center">
                      <KYCStatusBadge kyc={client.kyc} />
                    </TableCell>

                    <TableCell className="px-4 py-3 text-sm text-center">{client.alerts}</TableCell>

                    <TableCell className="px-4 py-3 text-sm text-left font-medium">
                      <span
                        className="inline-flex truncate"
                        title={String(client.balance)}
                      >
                        {client.balance}
                      </span>
                    </TableCell>
                    
                    <TableCell className="px-4 py-3 text-sm text-left text-slate-600">
                      {formatDateTime(client.lastActivity)}
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
                )}))}
        </TableBody>
      </Table>
    </div>
  );
}
