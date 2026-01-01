// Imports Padrões do React
import { useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
// Imagens da UBS
import UbsLogo from "@/assets/svg/ubs_logo.svg";
import loginBg from "@/assets/png/ubs.jpg";

import { useAuthStore } from "@/store/auth";

// Shadcn/ui
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

// Icones
import {
  AlertTriangle,
  Clock,
  CheckCircle
} from "lucide-react";

import { AlertsBySeverityChart } from "@/components/ui/charts/alertsbyseveritychart";
import { TransactionsByTypeChart } from "@/components/ui/charts/transactionchart";
import { WeeklyActivityChart } from "@/components/ui/charts/weeklyactivitychart";
import { ChartCard } from "@/components/ui/charts/chartcard";
import { AlertsTable } from "@/components/ui/tables/alertstable";
import { StatCard    } from "@/components/ui/statcard";
import { alertsMock } from "@/mocks/mocks";


type SeverityFilter = "all" | "high" | "medium" | "low";

export function AlertsPage() {

    const [severity, setSeverity] = useState<SeverityFilter>("all");
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

  return (
    
    <div
      className="relative bg-cover bg-center"
    >
      {/* Conteúdo */}
     <div className="relative z-10 p-6">

        {/* Grid */}
        <div className="grid gap-6 [grid-template-columns:repeat(auto-fit,minmax(240px,1fr))]">
          
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

        </div>

        {/* Filtro por Severidade */}
        <div className="mt-6 flex flex-wrap items-center gap-2 rounded-xl bg-white p-4 shadow">
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

        <div className="mt-5 rounded-xl bg-white p-6 shadow">
          <ChartCard title="Recent High-Priority Alerts">
            <AlertsTable severityFilter={severity}/>
          </ChartCard>
        </div>
      </div>
    </div>
  );
}

    