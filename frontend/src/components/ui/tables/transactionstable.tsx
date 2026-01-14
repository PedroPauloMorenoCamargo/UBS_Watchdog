import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useEffect, useRef } from "react";
import type { TransactionRow } from "@/models/transaction";
import { formatCurrencyCompact } from "@/lib/utils";

interface TransactionsTableProps {
  transactions: TransactionRow[];
  selectedId?: string | null;
  onSelect?: (id: string | null) => void;
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
        onSelect?.(null);
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
            <TableHead className="px-4 py-3">ID</TableHead>
            <TableHead className="px-4 py-3 text-center">Date</TableHead>
            <TableHead className="px-4 py-3 text-left">Amount</TableHead>
            <TableHead className="px-4 py-3">Type</TableHead>
            <TableHead className="px-4 py-3">Sender Id to Receiver</TableHead>
            <TableHead className="px-4 py-3">Country</TableHead>
        
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
                    onSelect?.(selected ? null : transaction.id)
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

                  <TableCell className="px-4 py-3 text-center text-slate-600">
                    {transaction.date}
                  </TableCell>

                  <TableCell className="px-4 py-3 text-left font-medium">
                      {formatCurrencyCompact(transaction.rawAmount)} {transaction.currencyCode}
                  </TableCell>

                  <TableCell className="px-4 py-3 text-slate-700">
                    {transaction.type}
                  </TableCell>

                  <TableCell className="px-4 py-3">
                    <div className="flex flex-col text-sm">
                      <span className="font-medium text-slate-700">
                        {transaction.clientId}
                      </span>
                      <span className="text-slate-500">to </span>
                      <span className="text-slate-500 font-medium">
                        {transaction.counterIdentifier}
                      </span>
                    </div>
                  </TableCell>

                  <TableCell className="px-4 py-3 text-slate-600">
                    {transaction.country}
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
