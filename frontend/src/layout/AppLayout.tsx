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
    <div className="flex min-h-screen w-full overflow-x-hidden">
      <Navbar />

      <div className="flex flex-col flex-1 min-w-0">
        <Header />

        <main className="flex-1 bg-slate-100 p-6 min-w-0 overflow-x-hidden">
          <h3 className="mb-1 text-xl font-semibold leading-tight">
            {title}</h3>
          <Outlet />
        </main>
      </div>
    </div>
  );
}
