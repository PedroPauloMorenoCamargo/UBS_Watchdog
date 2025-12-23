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

export function Dashboard() {
  return (
    <div
      className="relative min-h-screen bg-cover bg-center"
      style={{ backgroundImage: `url(${loginBg})` }}
    >
      {/* Overlay */}
      <div className="absolute inset-0 bg-black/60" />

      {/* Conteúdo */}
      <div className="relative z-10 p-6">
        {/* Título */}
        <h2 className="mb-6 text-2xl font-bold text-white 
                       drop-shadow-[0_2px_6px_rgba(255,255,255,0.8)]">
          ISSO AQUI TA FICANDO MANEIRO
        </h2>

        {/* Grid */}
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
          
          {/* Card 1 */}
          <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-2 text-lg font-semibold">Total Clients</h3>
            <p className="text-3xl font-bold text-blue-600">1,245</p> {/*mock*/}
          </div>

          {/* Card 2 */}
          <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-2 text-lg font-semibold">Active Reports</h3>
            <p className="text-3xl font-bold text-green-600">87</p> {/*mock*/}
          </div>

          {/* Card 3 — Filtros */}
<div className="rounded-xl bg-white/90 p-6 shadow-lg">
  <h3 className="mb-4 text-lg font-semibold">Filters</h3>

  {/* Search */}
  <div className="relative mb-3 w-full">
    <Search
      size={18}
      className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400"
    />
    <input
      type="text"
      placeholder="Search..."
      className="w-full rounded-md border bg-slate-50
                 pl-3 pr-10 py-2
                 focus:outline-none focus:ring-2 focus:ring-blue-500"
    />
  </div>

            {/* Dropdowns */}
            <select className="mb-3 w-full rounded-md border px-3 py-2">
              <option>Status</option>
              <option>Active</option>
              <option>Inactive</option>
            </select>

            <select className="w-full rounded-md border px-3 py-2">
              <option>Period</option>
              <option>Last 7 days</option>
              <option>Last 30 days</option>
            </select>
          </div>

          {/* Card 4 — Gráfico */}
          <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-4 text-lg font-semibold">Reports Overview</h3>

            {/* Placeholder do gráfico */}
            <div className="flex h-40 items-center justify-center rounded-md bg-slate-100 text-slate-500">
              Chart goes here
            </div>
          </div>

        </div>
      </div>
    </div>
  );
}

    