"use client";
import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useMemo, useState, useCallback } from "react";
import type { SeverityFilter } from "@/types/alert";
import { mapTransactionToRow } from "@/mappers/transaction/transaction.mapper";

import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";

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
import { Pagination } from "@/components/ui/pagination";

const PAGE_SIZE = 20;

export function TransactionsPage() {
  const [type, setType] = useState<"all" | "Transfer" | "Deposit" | "Withdrawal">("all");
  const [minAmount, setMinAmount] = useState("");
  const [maxAmount, setMaxAmount] = useState("");
  const [search, setSearch] = useState("");
  const [severity, setSeverity] = useState<SeverityFilter>("all");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");

  const [selectedTransactionId, setSelectedTransactionId] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);

  const fetchTransactionsWithPagination = useCallback(
    () => fetchTransactions({ page: currentPage, pageSize: PAGE_SIZE }),
    [currentPage]
  );

  const [createOpen, setCreateOpen] = useState(false);
  const [importOpen, setImportOpen] = useState(false);

  const { data, loading, error, refetch } = useApi({
    fetcher: fetchTransactions,
  const { data, loading, error } = useApi({
    fetcher: fetchTransactionsWithPagination,
    deps: [currentPage],
  });

  const transactions = useMemo(() => {
    if (!data) return [];
    return data.items.map(mapTransactionToRow);
  }, [data]);

  const filteredTransactions = useMemo(() => {
  return transactions.filter((t) => {

    // 🔹 Search (clientId ou cpName)
    const searchMatch =
      !search ||
      t.counterIdentifier?.toLowerCase().includes(search.toLowerCase()) ||
      t.clientId?.toLowerCase().includes(search.toLowerCase());

    // 🔹 Type
    const typeMatch = type === "all" || t.type === type;

    // 🔹 Amount
    const amount = t.rawAmount;
    const min = minAmount ? Number(minAmount) : null;
    const max = maxAmount ? Number(maxAmount) : null;
    const minMatch = min === null || amount >= min;
    const maxMatch = max === null || amount <= max;

    // 🔹 Date
    const transactionDate = t.rawTimestamp;

    const startMatch =
      !startDate ||
      transactionDate >= new Date(startDate + "T00:00:00").getTime();

    const endMatch =
      !endDate ||
      transactionDate <= new Date(endDate + "T23:59:59").getTime();
      console.log(endDate)

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
            <label className="text-xs font-medium text-slate-500">Type</label>
            <Select value={type} onValueChange={(v) => setType(v as any)}>
              <SelectTrigger className="h-9 w-full">
                <SelectValue placeholder="All" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All</SelectItem>
                <SelectItem value="Transfer">Wire Transfer</SelectItem>
                <SelectItem value="Deposit">Cash Deposit</SelectItem>
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

            <Button className="cursor-pointer hover:bg-slate-200"
              variant="outline" 
              onClick={() => console.log("Importar CSV")}>
              Import CSV
            </Button>
          </div>
      </div>

      <div className="mt-5">
        <ChartCard title="Recent Transactions">
          {loading && <p>Loading transactions...</p>}
          {error && <p className="text-red-500">{error}</p>}
          {!loading && !error && (
          <div className="relative">
            <TransactionsTable
              transactions={filteredTransactions}
              selectedId={selectedTransactionId}
              onSelect={setSelectedTransactionId}
            />
            {loading && (
              <div className="absolute inset-0 flex items-center justify-center bg-white/70 z-10">
                <span>Loading...</span>
              </div>
            )}
            {error && <p className="text-red-500 absolute top-2 left-2 z-20">{error}</p>}
          </div>
          {/* Pagination */}
          {data && (
            <Pagination
              currentPage={currentPage}
              totalPages={data.totalPages}
              totalItems={data.totalCount}
              pageSize={PAGE_SIZE}
              onPageChange={setCurrentPage}
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
