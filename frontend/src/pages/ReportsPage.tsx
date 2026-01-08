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

import { AdaptiveLineChart } from "@/components/ui/charts/adaptivelinechart";
import { ChartCard } from "@/components/ui/charts/chartcard";
import { ReportsTable } from "@/components/ui/tables/reportstable";
import { reportsMock } from "@/mocks/mocks";
import { weeklyAlertsBySeverity } from "@/mocks/mocks";

export function ReportsPage() {
  return (
    <div className="relative bg-cover bg-center">
     <div className="relative z-10 p-3">
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          
          <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-2 text-lg font-semibold">Average Resolution Time</h3>
            <p className="text-3xl font-bold text-black-600">4.2 hrs</p> {/*mock*/}
             <p className="text-sm mt-2 font-bold text-green-600">-12% vs. last month</p> {/*mock*/}
          </div>

          <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-2 text-lg font-semibold">Total Alerts Processed</h3>
            <p className="text-3xl font-bold text-black-600">1,248</p> {/*mock*/}
             <p className="text-sm mt-2 font-bold text-gray-500">Last 30 days</p> {/*mock*/}
          </div>
          
          <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-2 text-lg font-semibold">False Positive Rate</h3>
            <p className="text-3xl font-bold text-black-600">18%</p> {/*mock*/}
            <p className="text-sm mt-2 font-bold text-red-600">+2% vs. last month</p> {/*mock*/}
          </div>

        </div>
        
        <div className="mt-6">
          <ChartCard
            title="Alert Trends (Last 7 Days)"
          >
            <AdaptiveLineChart
                data={weeklyAlertsBySeverity}
                xKey="day"
                lines={[
                    { key: "high", label: "High", color: "#dc2626" },
                    { key: "medium", label: "Medium", color: "#f59e0b" },
                    { key: "low", label: "Low", color: "#16a34a" },
                ]}
            />
          </ChartCard>
        </div>

        <div className="mt-5">
          <ChartCard title="Generated Reports">
            <ReportsTable reports={reportsMock}/>
          </ChartCard>
        </div>
      </div>
    </div>
  );
}

    