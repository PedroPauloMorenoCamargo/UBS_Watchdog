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

        <main className="flex-1 bg-muted/40 p-8 min-w-0 overflow-x-hidden">
          <div className="mb-8 flex items-center justify-between">
             <h3 className="text-2xl font-semibold tracking-tight text-foreground">
              {title}
            </h3>
          </div>
          <Outlet />
        </main>
      </div>
    </div>
  );
}
