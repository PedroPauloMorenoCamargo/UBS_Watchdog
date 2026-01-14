import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useMemo, useState, useCallback } from "react";
import { ChartCard } from "@/components/ui/charts/chartcard";
import { TransactionsTable } from "@/components/ui/tables/transactionstable";
import { useApi } from "@/hooks/useApi";
import { fetchTransactions } from "@/services/transaction.service";
import { mapTransactionToRow } from "@/mappers/transaction/transaction.mapper";
// import { RegisterTransactionDialog } from "@/components/ui/dialogs/register-transaction-dialog";
import { CreateTransactionDialog } from "@/components/ui/dialogs/create-transaction-dialog";
import { ImportTransactionsCsvDialog } from "@/components/ui/dialogs/import-transactions-csv-dialog";
import { Pagination } from "@/components/ui/pagination";
import { ToastContainer } from "@/components/ui/toast";
import { useToast } from "@/hooks/useToast";

const PAGE_SIZE = 20;

export function TransactionsPage() {
  const [type, setType] = useState<"all" | "Wire" | "Deposit" | "Withdrawal">("all");
  const [minAmount, setMinAmount] = useState("");
  const [maxAmount, setMaxAmount] = useState("");
  const [search, setSearch] = useState("");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [selectedTransactionId, setSelectedTransactionId] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [registerDialogOpen, setRegisterDialogOpen] = useState(false);

  const { toasts, removeToast, success } = useToast();

  const fetchTransactionsWithPagination = useCallback(
    () => fetchTransactions({ page: currentPage, pageSize: PAGE_SIZE }),
    [currentPage]
  );

  const [importOpen, setImportOpen] = useState(false);
  const { data, loading, error, refetch } = useApi({
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

  return (
    <div className="relative">
      <ToastContainer toasts={toasts} onRemove={removeToast} />
      
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
                <SelectItem value="Wire">Wire Transfer</SelectItem>
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

      <div className="mt-6 flex flex-wrap items-center gap-2 rounded-xl bg-white p-4 shadow">
        <Button
          className="cursor-pointer hover:bg-slate-600"
          onClick={() => setRegisterDialogOpen(true)}
        >
          Create Transaction
        </Button>

        <Button
          className="cursor-pointer hover:bg-slate-200"
          variant="outline"
          onClick={() => setImportOpen(true)}
        >
          Import CSV
        </Button>
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

              {error && (
                <p className="text-red-500 absolute top-2 left-2 z-20">
                  {error}
                </p>
              )}
            </div>
          )}

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


      <CreateTransactionDialog
        isOpen={registerDialogOpen}
        onClose={() => setRegisterDialogOpen(false)}
        onCreated={() => {
          setRegisterDialogOpen(false);
          refetch();
          success("Transação criada com sucesso!");
        }}
      />

      <ImportTransactionsCsvDialog
        isOpen={importOpen}
        onClose={() => setImportOpen(false)}
      />
    </div>)
}