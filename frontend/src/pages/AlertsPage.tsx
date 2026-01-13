// AlertsPage.tsx
import { useMemo, useState } from "react";
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

import type { SeverityFilter } from "@/types/alert";
import type { StatusFilter } from "@/types/status";

import { useApi } from "@/hooks/useApi";
import { fetchCases } from "@/services/case.service";
import { mapCaseDtoToTableRow } from "@/mappers/case/case.mapper";

export function AlertsPage() {
  const [severity, setSeverity] = useState<SeverityFilter>("all");
  const [status, setStatus] = useState<StatusFilter>("all");

  // Buscar dados do backend
  const { data, loading, error } = useApi({
    fetcher: fetchCases,
  });

  // Mapear dados do backend para formato da tabela
  const cases = useMemo(() => {
    if (!data) return [];
    return data.items.map(mapCaseDtoToTableRow);
  }, [data]);

  // Contagem por severidade
  const severityCounts = useMemo(() => {
    const counts = {
      all: 0,
      critical: 0,
      high: 0,
      medium: 0,
      low: 0,
    };

   cases.forEach((c) => {
  const sev = c.severity; 
  counts.all += 1;
  counts[sev as keyof typeof counts] += 1; 
});

    return counts;
  }, [cases]);

  // Filtrar casos por severidade e status
  const filteredCases = useMemo(() => {
    return cases.filter((c) => {
      const severityMatch = severity === "all" || c.severity === severity;
      const statusMatch = status === "all" || c.status === status;
      return severityMatch && statusMatch;
    });
  }, [cases, severity, status]);

  return (
    <div className="relative bg-cover bg-center">
      {/* Cards de contagem */}
      <div className="grid gap-6 [grid-template-columns:repeat(auto-fit,minmax(240px,1fr))]">
        <StatCard title="Critical Severity" value={severityCounts.critical} variant="destructive" />
        <StatCard title="High Severity" value={severityCounts.high} variant="high" />
        <StatCard title="Medium Severity" value={severityCounts.medium} variant="medium" />
        <StatCard title="Low Severity" value={severityCounts.low} variant="low" />
      </div>

      {/* Filtros */}
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
          All ({severityCounts.all})
        </Button>

        <Button
          variant={severity === "critical" ? "destructive" : "outline"}
          size="sm"
          onClick={() => setSeverity("critical")}
        >
          Critical ({severityCounts.critical})
        </Button>

        <Button
          variant={severity === "high" ? "destructive" : "outline"}
          size="sm"
          onClick={() => setSeverity("high")}
        >
          High ({severityCounts.high})
        </Button>

        <Button
          variant={severity === "medium" ? "default" : "outline"}
          size="sm"
          onClick={() => setSeverity("medium")}
        >
          Medium ({severityCounts.medium})
        </Button>

        <Button
          variant={severity === "low" ? "default" : "outline"}
          size="sm"
          onClick={() => setSeverity("low")}
        >
          Low ({severityCounts.low})
        </Button>
      </div>

      {/* Tabela de Alerts */}
      <div className="mt-5">
        <ChartCard title="Recent High-Priority Alerts">
          {loading && <p>Loading...</p>}
          {error && <p className="text-red-500">{error}</p>}
          {!loading && !error && <AlertsTable alerts={filteredCases} />}
        </ChartCard>
      </div>
    </div>
  );
}
