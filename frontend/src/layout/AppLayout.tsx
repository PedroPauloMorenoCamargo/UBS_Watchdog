import { Outlet } from "react-router-dom";
import { Navbar } from "@/components/ui/navbar";
import { Header } from "@/components/ui/header";

export function AppLayout() {
  return (
    <div className="flex">
      <Navbar />

      <div className="flex flex-col flex-1 min-h-screen">
        <Header />

        <main className="flex-1 bg-slate-100 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
