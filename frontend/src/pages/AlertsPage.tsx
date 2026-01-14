// AlertsPage.tsx
import { useMemo, useState, useCallback } from "react";
import { useCases } from "@/hooks/useCases";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";

import { ChartCard } from "@/components/ui/charts/chartcard";
import { AlertsTable } from "@/components/ui/tables/alertstable";
import { StatCard } from "@/components/ui/statcard";
import { Pagination } from "@/components/ui/pagination";

import type { SeverityFilter } from "@/types/alert";
import type { StatusFilter } from "@/types/status";

import { useApi } from "@/hooks/useApi";
import { fetchCases } from "@/services/case.service";
import { mapCaseDtoToTableRow } from "@/mappers/case/case.mapper";

const PAGE_SIZE = 20;

export function AlertsPage() {
  const [severity, setSeverity] = useState<SeverityFilter>("all");
  const [status, setStatus] = useState<StatusFilter>("all");
  const [currentPage, setCurrentPage] = useState(1);

  const fetchCasesWithPagination = useCallback(
    () => fetchCases({ page: currentPage, pageSize: PAGE_SIZE }),
    [currentPage]
  );

  const { data, loading, error, refetch } = useApi({
    fetcher: fetchCasesWithPagination,
    deps: [currentPage],
  });


  const fetchAllCasesForStats = useCallback(
    () => fetchCases({ page: 1, pageSize: 2000 }),
    []
  );

  const { data: statsData } = useApi({
    fetcher: fetchAllCasesForStats,
  });


  const cases = useMemo(() => {
    if (!data) return [];
    return data.items.map(mapCaseDtoToTableRow);
  }, [data]);


  //  Transform Global Data for the Hook
  const globalCasesForStats = useMemo(() => {
    if (!statsData) return [];
    return statsData.items.map(mapCaseDtoToTableRow);
  }, [statsData]);

  // Use the Hook to calculate stats
  const { 
    activeAlertsCount, 
    criticalAlertsCount, 
    highAlertsCount, 
    mediumAlertsCount, 
    lowAlertsCount 
  } = useCases(globalCasesForStats);

  const filteredCases = useMemo(() => {
    return cases.filter((c) => {
      const severityMatch = severity === "all" || c.severity === severity;
      const statusMatch = status === "all" || c.status === status;
      return severityMatch && statusMatch;
      
    });
  }, [cases, severity, status]);

  return (
    <div className="relative bg-cover bg-center">
      <div className="grid gap-6 [grid-template-columns:repeat(auto-fit,minmax(240px,1fr))]">
        <StatCard title="Critical Severity" value={criticalAlertsCount} variant="destructive" />
        <StatCard title="High Severity" value={highAlertsCount} variant="high" />
        <StatCard title="Medium Severity" value={mediumAlertsCount} variant="medium" />
        <StatCard title="Low Severity" value={lowAlertsCount} variant="low" />
        
      </div>

      <div className="mt-6 flex flex-wrap items-center gap-2 rounded-xl bg-white p-4 shadow">
        <div className="flex items-center gap-2">
          <label className="text-xs font-medium text-slate-500">Status:</label>
          <Select value={status} onValueChange={(v) => setStatus(v as StatusFilter)}>
            <SelectTrigger className="h-9 min-w-[140px]">
              <SelectValue placeholder="All" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All</SelectItem>
              <SelectItem value="New">New</SelectItem>
              <SelectItem value="Under Review">Under Review</SelectItem>
              <SelectItem value="Resolved">Resolved</SelectItem>
              
            </SelectContent>
          </Select>
        </div>

        <span className="mr-2 text-sm font-semibold text-slate-600">Filter by severity:</span>

        <Button
          variant={severity === "all" ? "default" : "outline"}
          size="sm"
          onClick={() => setSeverity("all")}
        >
          All ({activeAlertsCount})
        </Button>

        <Button
          variant={severity === "critical" ? "destructive" : "outline"}
          size="sm"
          onClick={() => setSeverity("critical")}
        >
          Critical ({criticalAlertsCount})
        </Button>

        <Button
          variant={severity === "high" ? "destructive" : "outline"}
          size="sm"
          onClick={() => setSeverity("high")}
        >
          High ({highAlertsCount})
        </Button>

        <Button
          variant={severity === "medium" ? "default" : "outline"}
          size="sm"
          onClick={() => setSeverity("medium")}
        >
          Medium ({mediumAlertsCount})
        </Button>

        <Button
          variant={severity === "low" ? "default" : "outline"}
          size="sm"
          onClick={() => setSeverity("low")}
        >
          Low ({lowAlertsCount})
        </Button>
      </div>

      <div className="mt-5">
        <ChartCard title="Recent High-Priority Alerts">
          {loading && !data && <p>Loading...</p>}
          {error && <p className="text-red-500">{error}</p>}
          {data && (
            <div className={loading ? "opacity-60 transition-opacity duration-200" : "transition-opacity duration-200"}>
               <AlertsTable alerts={filteredCases} onRefresh={refetch} />
            </div>
          )}
          
          {!loading && !error && data && (
            <Pagination
              currentPage={currentPage}
              totalPages={data.totalPages}
              totalItems={data.total}
              pageSize={PAGE_SIZE}
              onPageChange={setCurrentPage}
            />
          )}
        </ChartCard>
      </div>
    </div>
  );
}
