import { Outlet, useMatches } from "react-router-dom";
import { Navbar } from "@/components/ui/navbar";
import { Header } from "@/components/ui/header";
import type { RouteHandle } from "@/types/handle";

export function AppLayout() {

  const matches = useMatches();

  const currentRoute = matches[matches.length - 1];
  const handle = currentRoute?.handle as RouteHandle | undefined;
  const title = handle?.title ?? "";

  return (
    <div className="flex min-h-screen">
      <Navbar />

      <div className="flex flex-col flex-1">
        <Header />

        <main className="flex-1 bg-slate-100 p-6">
          <h3 className="text-xl font-semibold mb-1 leading-tight">
            {title}</h3>
          <Outlet />
        </main>
      </div>
    </div>
  );
}
