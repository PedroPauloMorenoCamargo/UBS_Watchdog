import { ChartCard } from "@/components/ui/charts/chartcard";
import { AlertsTable } from "@/components/ui/tables/alertstable";

import {
  Users,
  ShieldCheck,
  Settings,
  HeartPulse,
} from "lucide-react";


export function AdminPage() {

  return (
    
    <div
      className="relative bg-cover bg-center"
    >
      {/* Conteúdo */}
     <div className="relative z-10 p-3">

        {/* Grid */}
        <div className="grid gap-6 [grid-template-columns:repeat(auto-fit,minmax(240px,1fr))]">
          
          
        </div>

        <div className="mt-2 rounded-xl bg-white p-2 shadow">
            <div className="grid grid-cols-1 gap-3 place-items-center sm:grid-cols-2 lg:grid-cols-4">
                {/* Users */}
                <div className="flex items-center gap-3 hover:bg-slate-50 cursor-pointer">
                <Users className="h-4 w-4 text-slate-600" />
                <span className="font-medium text-slate-700">Users</span>
                </div>

                {/* Rules */}
                <div className="flex items-center gap-3 hover:bg-slate-50 cursor-pointer">
                <ShieldCheck className="h-4 w-4 text-slate-600" />
                <span className="font-medium text-slate-700">Rules</span>
                </div>

                {/* Settings */}
                <div className="flex items-center gap-3 hover:bg-slate-50 cursor-pointer">
                <Settings className="h-4 w-4 text-slate-600" />
                <span className="font-medium text-slate-700">Settings</span>
                </div>

                {/* Audit Log */}
                <div className="flex items-center gap-3 hover:bg-slate-50 cursor-pointer">
                <HeartPulse className="h-4 w-4 text-slate-600" />
                <span className="font-medium text-slate-700">Audit Log</span>
                </div>
            </div>
        </div>

        <div className="mt-5">
          <ChartCard title="Recent High-Priority Admin">
            <AlertsTable severityFilter={"all"}/>
          </ChartCard>
        </div>
      </div>
    </div>
  );
}

    