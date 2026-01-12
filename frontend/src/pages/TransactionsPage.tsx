// Shadcn/ui
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useMemo,useState } from "react";
import type { SeverityFilter } from "@/types/alert";
import { transactionsMock } from "@/mocks/mocks";
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

export function TransactionsPage() {
  const [type, setType] = useState<"all" | "Wire Transfer" | "Cash Deposit">("all");
  const [minAmount, setMinAmount] = useState("");
  const [maxAmount, setMaxAmount] = useState("");
  const [search, setSearch] = useState("");
  const [severity, setSeverity] = useState<SeverityFilter>("all");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [selectedTransactionId, setSelectedTransactionId] = useState<string | null>(null);

  const { data, loading, error } = useApi({
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
        
      const typeMatch =
        type === "all" || t.type === type;

      const amount = t.rawAmount;
      const min = minAmount ? Number(minAmount) : null;
      const max = maxAmount ? Number(maxAmount) : null;

      const minMatch = min === null || amount >= min;
      const maxMatch = max === null || amount <= max;

      const transactionDate = new Date(t.date).getTime();

      const startMatch =
        !startDate || transactionDate >= new Date(startDate).getTime();

      const endMatch =
        !endDate || transactionDate <= new Date(endDate).getTime();

      return searchMatch && typeMatch && minMatch && maxMatch && startMatch && endMatch;
    });
  }, [transactions,search, severity, type, minAmount, maxAmount, startDate, endDate]);


  return (
    <div className="relative">
      <div className="mt-4 rounded-xl bg-white p-5 shadow">
        
        <div
          className="grid 
          gap-4 
          items-end
          grid-cols-1
          sm:grid-cols-2
          lg:grid-cols-3
          xl:grid-cols-4"
        >
          <div>
            <label className="text-xs font-medium text-slate-500">
              Search Parties
            </label>
            <Input
              placeholder="Sender Client Id or Receiver name..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="h-9 rounded-none border-0 border-b border-slate-300 px-0 shadow-none focus-visible:ring-0"
            />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">
              Severity
            </label>
            <Select 
              value={severity} 
              onValueChange={(v) => setSeverity(v as SeverityFilter)}
            >
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
            <label className="text-xs font-medium text-slate-500">
              Type
            </label>
            <Select value={type} onValueChange={(v) => setType(v as any)}>
              <SelectTrigger className="h-9 w-full">
                <SelectValue placeholder="All" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All</SelectItem>
                <SelectItem value="Wire Transfer">Wire Transfer</SelectItem>
                <SelectItem value="Cash Deposit">Cash Deposit</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">
              Start date
            </label>
            <Input 
              type="date" 
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              className="h-9 w-full" />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">
              End date
            </label>
            <Input 
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              className="h-9 w-full"
            />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">
              Min amount
            </label>
            <Input
              type="number"
              placeholder="0"
              value={minAmount}
              onChange={(e) => setMinAmount(e.target.value)}
              className="h-9 w-full"
            />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">
              Max amount
            </label>
            <Input
              type="number"
              placeholder="10000"
              value={maxAmount}
              onChange={(e) => setMaxAmount(e.target.value)}
              className="h-9 w-full"
            />
          </div>

          <div className="flex justify-between">
            <Button variant="outline"
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
            <Button className="cursor-pointer hover:bg-slate-600"
              onClick={() => console.log("Criar")}>
              Create Transaction
            </Button>

            <Button className="cursor-pointer hover:bg-slate-200"
              variant="outline" 
              onClick={() => console.log("Importar CSV")}>
              Import CSV
            </Button>

            <Button className="cursor-pointer hover:bg-slate-200"
              variant="outline"
              disabled={!selectedTransactionId}
              onClick={() => console.log("Editar", selectedTransactionId)}>
              Edit Transaction
            </Button>
            
            <Button className="cursor-pointer hover:bg-slate-600"
              variant="destructive"
              disabled={!selectedTransactionId}
              onClick={() => console.log("Excluir", selectedTransactionId)}
            >
              Delete Transaction
            </Button>
          </div>
      </div>

      <div className="mt-5">
        <ChartCard title="Recent Transactions">
          {loading && <p>Loading transactions...</p>}
          {error && <p className="text-red-500">{error}</p>}
          {!loading && !error && (
            
              <TransactionsTable transactions={filteredTransactions} 
                selectedId={selectedTransactionId}
                onSelect={setSelectedTransactionId}/>
          )}
        </ChartCard>
      </div>
    </div>
  );
}

    