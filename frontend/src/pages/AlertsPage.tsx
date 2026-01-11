// Imports Padrões do React
import { useMemo, useState } from "react";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
// Shadcn/ui
import { Button } from "@/components/ui/button";

import { ChartCard } from "@/components/ui/charts/chartcard";
import { AlertsTable } from "@/components/ui/tables/alertstable";
import { StatCard    } from "@/components/ui/statcard";
import { alertsMock } from "@/mocks/mocks";
import type { SeverityFilter } from "@/types/alert";
import type { StatusFilter } from "@/types/status";

export function AlertsPage() {

    const [severity, setSeverity] = useState<SeverityFilter>("all");
    const [status, setStatus] = useState<StatusFilter>("all");

    const severityCounts = useMemo(() => {
    return alertsMock.reduce(
        (acc, alert) => {
        acc.all += 1;

        const sev = alert.severity.toLowerCase() as "high" | "medium" | "low";
        acc[sev] += 1;

        return acc;
        },
        {
        all: 0,
        high: 0,
        medium: 0,
        low: 0,
        }
    );
    }, []);

    const filteredAlerts = useMemo(() => {
      return alertsMock.filter((alert) => {
        const severityMatch =
          severity === "all" ||
          alert.severity.toLowerCase() === severity;

        const statusMatch =
          status === "all" ||
          alert.status.toLowerCase() === status;

        return severityMatch && statusMatch;
      });
    }, [severity, status]);



  return (

    <div
      className="relative bg-cover bg-center"
    >      
      <div className="grid gap-6 [grid-template-columns:repeat(auto-fit,minmax(240px,1fr))]">
        <StatCard
            title="High Severity"
            value={8}
            variant="high"
        />

        <StatCard
            title="Medium Severity"
            value={24}
            variant="medium"
        />

        <StatCard
            title="Low Severity"
            value={55}
            variant="low"
        />
      </div>
      
      <div className="mt-6 flex flex-wrap items-center gap-2 rounded-xl bg-white p-4 shadow">

        <div className="flex items-center gap-2">
          <label className="text-xs font-medium text-slate-500">
            Status:
          </label>
          <Select 
            value={status} 
            onValueChange={(v) => setStatus(v as StatusFilter)}
          >
            <SelectTrigger className="h-9 min-w-[140px]"> 
              <SelectValue placeholder="All" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All</SelectItem>
              <SelectItem value="new">New</SelectItem>
              <SelectItem value="analysis">Analysis</SelectItem>
              <SelectItem value="resolved">Resolved</SelectItem>
            </SelectContent>
          </Select>
        </div>
        
        <span className="mr-2 text-sm font-semibold text-slate-600">
            Filter by severity:
        </span>

        <Button
            variant={severity === "all" ? "default" : "outline"}
            size="sm"
            onClick={() => setSeverity("all")}
        >
            All ({severityCounts.all})
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

      <div className="mt-5">
        <ChartCard title="Recent High-Priority Alerts">
          <AlertsTable alerts={filteredAlerts}/>
        </ChartCard>
      </div>
    </div>
  );
}

    