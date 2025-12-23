import { Outlet } from "react-router-dom";
import { Navbar } from "@/components/ui/navbar";

export function AppLayout() {
  return (
    <div className="flex">
      <Navbar />
      <main className="flex-1 min-h-screen bg-slate-100">
        <Outlet />
      </main>
    </div>
  );
}
