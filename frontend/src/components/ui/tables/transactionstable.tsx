import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { SeverityBadge } from "../severitybadge";
import { transactionsMock } from "@/mocks/mocks";
import { useEffect, useRef } from "react";

interface TransactionsTableProps {
  transactions: typeof transactionsMock;
  selectedId: string | null;
  onSelect: (id: string | null) => void;
}

export function TransactionsTable({
  transactions,
  selectedId,
  onSelect,
}: TransactionsTableProps) {
  const tableRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        tableRef.current &&
        !tableRef.current.contains(event.target as Node)
      ) {
        onSelect(null);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [onSelect]);

  return (
    <div
      ref={tableRef}
      className="rounded-lg border bg-white max-h-[420px] overflow-y-auto"
    >
      <Table>
        <TableHeader className="sticky top-0 z-10 bg-white">
          <TableRow>
            <TableHead className="w-[140px]">ID</TableHead>
            <TableHead className="w-[110px] text-center">Date</TableHead>
            <TableHead className="w-[120px] text-left">Amount</TableHead>
            <TableHead className="w-[160px]">Type</TableHead>
            <TableHead>Parties</TableHead>
            <TableHead className="w-[100px]">Country</TableHead>
            <TableHead className="w-[90px] text-center">Risk</TableHead>
            <TableHead className="w-[110px] text-center">Status</TableHead>
          </TableRow>
        </TableHeader>

        <TableBody>
          {transactions.length === 0 ? (
            <TableRow>
              <TableCell
                colSpan={8}
                className="h-24 text-center text-sm text-slate-500"
              >
                No records found for selected filters.
              </TableCell>
            </TableRow>
          ) : (
            transactions.map((transaction, index) => {
              const selected = transaction.id === selectedId;

              return (
                <TableRow
                  key={`${transaction.id}-${index}`}
                  onClick={() =>
                    onSelect(selected ? null : transaction.id)
                  }
                  className={`
                    cursor-pointer
                    ${selected ? "bg-slate-100" : "hover:bg-slate-50"}
                  `}
                  aria-selected={selected}
                >
                  <TableCell className="w-[140px] font-medium text-slate-700">
                    {transaction.id}
                  </TableCell>

                  <TableCell className="w-[110px] text-center text-slate-600">
                    {transaction.date}
                  </TableCell>

                  <TableCell className="w-[120px] text-left font-medium">
                    <span
                      className="block max-w-[120px] truncate"
                      title={String(transaction.amount)}
                    >
                      {transaction.amount}
                    </span>
                  </TableCell>

                  <TableCell className="w-[160px] text-slate-700">
                    {transaction.type}
                  </TableCell>

                  <TableCell>
                    <div className="flex flex-col text-sm">
                      <span className="font-medium text-slate-700">
                        {transaction.parties.sender}
                      </span>
                      <span className="text-slate-500">
                        to {transaction.parties.receiver}
                      </span>
                    </div>
                  </TableCell>

                  <TableCell className="w-[100px] text-slate-600">
                    {transaction.country}
                  </TableCell>

                  <TableCell className="w-[90px] text-center">
                    <SeverityBadge severity={transaction.severity} />
                  </TableCell>

                  <TableCell className="w-[110px] text-center">
                    <span className="rounded-full bg-slate-100 px-2 py-1 text-xs font-medium text-slate-700">
                      {transaction.status}
                    </span>
                  </TableCell>
                </TableRow>
              );
            })
          )}
        </TableBody>
      </Table>
    </div>
  );
}
