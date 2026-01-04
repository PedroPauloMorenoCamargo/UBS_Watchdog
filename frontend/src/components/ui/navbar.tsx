import { NavLink } from "react-router-dom";
import {
  LayoutDashboard,
  Users,
  FileBarChart,
  Settings,
} from "lucide-react";

export function Navbar() {
  return (
    <aside className="w-64 bg-slate-900 text-white flex flex-col p-4">
      
      {/* Logo / Título */}
      <div className="text-xl font-bold mb-8">
        UBS Dashboard
      </div>

      {/* Menu */}
      <nav className="flex flex-col gap-2 flex-1">
        <NavLink
          to="/dashboard"
          className={({ isActive }) =>
            `flex items-center gap-3 rounded-md px-3 py-2 transition
             ${isActive ? "bg-slate-700" : "hover:bg-slate-800"}`
          }
        >
          <LayoutDashboard size={20} />
          Dashboard
        </NavLink>

        <NavLink
          to="/clients"
          className={({ isActive }) =>
            `flex items-center gap-3 rounded-md px-3 py-2 transition
             ${isActive ? "bg-slate-700" : "hover:bg-slate-800"}`
          }
        >
          <Users size={20} />
          Clients
        </NavLink>

          <NavLink
          to="/transactions"
          className={({ isActive }) =>
            `flex items-center gap-3 rounded-md px-3 py-2 transition
             ${isActive ? "bg-slate-700" : "hover:bg-slate-800"}`
          }
        >
          <FileBarChart size={20} />
          Transactions
        </NavLink>

          <NavLink
          to="/reports"
          className={({ isActive }) =>
            `flex items-center gap-3 rounded-md px-3 py-2 transition
             ${isActive ? "bg-slate-700" : "hover:bg-slate-800"}`
          }
        >
          <FileBarChart size={20} />
          Reports
        </NavLink>

        <NavLink
          to="/alerts"
          className={({ isActive }) =>
            `flex items-center gap-3 rounded-md px-3 py-2 transition
             ${isActive ? "bg-slate-700" : "hover:bg-slate-800"}`
          }
        >
          <FileBarChart size={20} />
          Alerts
        </NavLink>

        {/* Configurations no final */}
        <div className="pt-2 border-t border-slate-700">
          <NavLink
            to="/admin"
            className={({ isActive }) =>
              `flex items-center gap-3 rounded-md px-3 py-2 transition
               ${isActive ? "bg-slate-700" : "hover:bg-slate-800"}`
            }
          >
            <Settings size={20} />
            Admin
          </NavLink>
          </div>
      </nav>
    </aside>
  );
}