import { Outlet } from "react-router-dom";
import { Navbar } from "@/components/ui/navbar";
import { Header } from "@/components/ui/header";

export function AppLayout() {
  return (
    <div className="flex min-h-screen">
      <Navbar />

      <div className="flex flex-col flex-1">
        <Header />

        <main className="flex-1 bg-slate-100 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
