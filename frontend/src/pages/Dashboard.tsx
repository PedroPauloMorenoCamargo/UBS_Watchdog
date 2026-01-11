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
  Eye,
  EyeOff,
  Loader2,
  ShieldAlert,
  Users,
  ArrowLeftRight,
  ShieldCheck,
  FileBarChart,
  Search
} from "lucide-react";

import { AlertsBySeverityChart } from "@/components/ui/charts/alertsbyseveritychart";
import { TransactionsByTypeChart } from "@/components/ui/charts/transactionchart";
import { AdaptiveLineChart } from "@/components/ui/charts/adaptivelinechart";
import { weeklyActivity } from "@/mocks/mocks";
import { ChartCard } from "@/components/ui/charts/chartcard";
import { AlertsTable } from "@/components/ui/tables/alertstable";

export function Dashboard() {
  return (
    <div className="relative bg-cover bg-center">
     <div className="relative z-10 p-3">
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-2 text-lg font-semibold">Total Transactions</h3>
            <p className="text-3xl font-bold text-black-600">1,245</p> {/*mock*/}
          </div>

          <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-2 text-lg font-semibold">Alerts Today</h3>
            <p className="text-3xl font-bold text-red-600">87</p> {/*mock*/}
          </div>

          <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-4 text-lg font-semibold">High Risk Alerts</h3>
            <p className="text-3xl font-bold text-red-600">8</p> {/*mock*/}
          </div>
          
          <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-4 text-lg font-semibold">Monitored Clients</h3>
            <p className="text-3xl font-bold text-black-600">870</p> {/*mock*/}
          </div>

        </div>
        <div className="mt-5 grid grid-cols-1 gap-6 lg:grid-cols-2">
          <ChartCard title="Alerts by Severity">
            <AlertsBySeverityChart />
          </ChartCard>

          <ChartCard title="Transactions by Type">
            <TransactionsByTypeChart />
          </ChartCard>
        </div>

        <div className="mt-6">
          <ChartCard
            title="Weekly Activity Trend"
          >
            <AdaptiveLineChart
              data={weeklyActivity}
              xKey="day"
              lines={[
                { key: "alerts", label: "Alerts", color: "#dc2626" },
                { key: "transactions", label: "Transactions", color: "#2563eb" },
              ]}
            />
          </ChartCard>
        </div>

        <div className="mt-5 rounded-xl bg-white p-6 shadow">
          <ChartCard title="Recent High-Priority Alerts">
            <AlertsTable severityFilter="all"/>
          </ChartCard>
        </div>
      </div>
    </div>
  );
}

    