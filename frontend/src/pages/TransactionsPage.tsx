"use client";
import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { ChartCard } from "@/components/ui/charts/chartcard";
import { TransactionsTable } from "@/components/ui/tables/transactionstable";
import { useApi } from "@/hooks/useApi";
import { fetchTransactions } from "@/services/transaction.service";
import { mapTransactionToRow } from "@/mappers/transaction/transaction.mapper";
import type { SeverityFilter } from "@/types/alert";
import { CreateTransactionDialog } from "@/components/ui/dialogs/create-transaction-dialog";
import { ImportTransactionsCsvDialog } from "@/components/ui/dialogs/import-transactions-csv-dialog";

export function TransactionsPage() {
  const [type, setType] = useState<"all" | "Wire Transfer" | "Cash Deposit" | "Withdrawal">("all");
  const [minAmount, setMinAmount] = useState("");
  const [maxAmount, setMaxAmount] = useState("");
  const [search, setSearch] = useState("");
  const [severity, setSeverity] = useState<SeverityFilter>("all");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");

  const [selectedTransactionId, setSelectedTransactionId] = useState<string | null>(null);

  const [createOpen, setCreateOpen] = useState(false);
  const [importOpen, setImportOpen] = useState(false);

  const { data, loading, error, refetch } = useApi({
    fetcher: fetchTransactions,
  });

  const transactions = useMemo(() => {
    if (!data) return [];
    return data.items.map(mapTransactionToRow);
  }, [data]);

  const filteredTransactions = useMemo(() => {
    return transactions.filter((t) => {
      const searchMatch =
        !search ||
        t.counterPartyName?.toLowerCase().includes(search.toLowerCase()) ||
        t.clientId?.toLowerCase().includes(search.toLowerCase());

      const typeMatch = type === "all" || t.type === type;

      const amount = t.rawAmount;
      const min = minAmount ? Number(minAmount) : null;
      const max = maxAmount ? Number(maxAmount) : null;

      const minMatch = min === null || amount >= min;
      const maxMatch = max === null || amount <= max;

      const transactionDate = new Date(t.date).getTime();
      const startMatch = !startDate || transactionDate >= new Date(startDate).getTime();
      const endMatch = !endDate || transactionDate <= new Date(endDate).getTime();

      return searchMatch && typeMatch && minMatch && maxMatch && startMatch && endMatch;
    });
  }, [transactions, search, type, minAmount, maxAmount, startDate, endDate]);

  // exemplo: accountId fixo para criar transaction
  // retirar e deixar accountid normal 
  const selectedAccountId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";

  return (
    <div className="relative">
      <div className="mt-4 rounded-xl bg-white p-5 shadow">
        <div className="grid gap-4 items-end grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          <div>
            <label className="text-xs font-medium text-slate-500">Search Parties</label>
            <Input
              placeholder="Sender Client Id or Receiver name..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="h-9 rounded-none border-0 border-b border-slate-300 px-0 shadow-none focus-visible:ring-0"
            />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Severity</label>
            <Select value={severity} onValueChange={(v) => setSeverity(v as SeverityFilter)}>
              <SelectTrigger className="h-9 w-full">
                <SelectValue placeholder="All" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All</SelectItem>
                <SelectItem value="high">High</SelectItem>
                <SelectItem value="medium">Medium</SelectItem>
                <SelectItem value="low">Low</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Type</label>
            <Select value={type} onValueChange={(v) => setType(v as any)}>
              <SelectTrigger className="h-9 w-full">
                <SelectValue placeholder="All" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All</SelectItem>
                <SelectItem value="Wire Transfer">Wire Transfer</SelectItem>
                <SelectItem value="Cash Deposit">Cash Deposit</SelectItem>
                <SelectItem value="Withdrawal">Withdrawal</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Start date</label>
            <Input
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              className="h-9 w-full"
            />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">End date</label>
            <Input
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              className="h-9 w-full"
            />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Min amount</label>
            <Input
              type="number"
              placeholder="0"
              value={minAmount}
              onChange={(e) => setMinAmount(e.target.value)}
              className="h-9 w-full"
            />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Max amount</label>
            <Input
              type="number"
              placeholder="10000"
              value={maxAmount}
              onChange={(e) => setMaxAmount(e.target.value)}
              className="h-9 w-full"
            />
          </div>

          <div className="flex justify-between">
            <Button
              variant="outline"
              className="h-9"
              onClick={() => {
                setSearch("");
                setSeverity("all");
                setType("all");
                setMinAmount("");
                setMaxAmount("");
                setStartDate("");
                setEndDate("");
              }}
            >
              Clear filters
            </Button>
          </div>
        </div>
      </div>

      <div className="mt-4 rounded-xl bg-white p-4 shadow">
        <div className="flex flex-wrap items-center gap-3">
          <Button
            onClick={() => setCreateOpen(true)}
            className="cursor-pointer hover:bg-slate-600"
          >
            Create Transaction
          </Button>

          <Button
            variant="outline"
            className="cursor-pointer hover:bg-slate-200"
            onClick={() => setImportOpen(true)}
          >
            Import CSV
          </Button>

          <Button
            variant="outline"
            className="cursor-pointer hover:bg-slate-200"
            disabled={!selectedTransactionId}
            onClick={() => console.log("Editar", selectedTransactionId)}
          >
            Edit Transaction
          </Button>

        
        </div>
      </div>

      <div className="mt-5">
        <ChartCard title="Recent Transactions">
          {loading && <p>Loading transactions...</p>}
          {error && <p className="text-red-500">{error}</p>}
          {!loading && !error && (
            <TransactionsTable
              transactions={filteredTransactions}
              selectedId={selectedTransactionId}
              onSelect={setSelectedTransactionId}
            />
          )}
        </ChartCard>
      </div>

      {/* Modais */}
      <CreateTransactionDialog
        isOpen={createOpen}
        onClose={() => setCreateOpen(false)}
        accountId={selectedAccountId}
        onCreated={() => refetch()}
      />

      <ImportTransactionsCsvDialog
        isOpen={importOpen}
        onClose={() => setImportOpen(false)}
        onImported={() => refetch()}
      />
    </div>
  );
}
