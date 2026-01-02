// Shadcn/ui
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useMemo,useState } from "react";
import type { SeverityFilter } from "@/types/alert";
import { transactionsMock } from "@/mocks/mocks";

import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";

import { ChartCard } from "@/components/ui/charts/chartcard";
import { TransactionsTable } from "@/components/ui/tables/transactionstable";

export function TransactionsPage() {
  const [search, setSearch] = useState("");
  const [severity, setSeverity] = useState<SeverityFilter>("all");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");

  const filteredTransactions = useMemo(() => {
  return transactionsMock.filter((t) => {

      const searchMatch =
        !search ||
        t.parties.sender.toLowerCase().includes(search.toLowerCase()) ||
        t.parties.receiver.toLowerCase().includes(search.toLowerCase());

      
      const severityMatch =
        severity === "all" || t.severity.toLowerCase() === severity;

      const transactionDate = new Date(t.date).getTime();

      const startMatch =
        !startDate || transactionDate >= new Date(startDate).getTime();

      const endMatch =
        !endDate || transactionDate <= new Date(endDate).getTime();

      return searchMatch && severityMatch && startMatch && endMatch;
    });
  }, [search, severity, startDate, endDate]);


  return (
    <div className="relative">
      <div className="mt-4 rounded-xl bg-white p-5 shadow">
        
        <div
          className="grid items-end gap-4"
          style={{ gridTemplateColumns: "2fr 1.5fr 1.5fr 1.5fr auto" }}
        >
          <div>
            <label className="text-xs font-medium text-slate-500">
              Search Parties
            </label>
            <Input
              placeholder="Sender or receiver name..."
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

          <div className="flex justify-end">
            <Button variant="outline"
              className="h-9"
              onClick={() => {
                setSearch("");
                setSeverity("all");
                setStartDate("");
                setEndDate("");
              }}
            >
              Clear
            </Button>
          </div>
        </div>
      </div>
        <div className="mt-5">
          <ChartCard title="Recent High-Priority Alerts">
            <TransactionsTable transactions={filteredTransactions}/>
          </ChartCard>
        </div>
    </div>
  );
}

    