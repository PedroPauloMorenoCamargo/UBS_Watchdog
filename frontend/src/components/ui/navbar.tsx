import { NavLink } from "react-router-dom";
import {
  LayoutDashboard,
  Users,
  FileBarChart,
  Settings,
  AlertCircle,
  ArrowLeftRight,
} from "lucide-react";

export function Navbar() {
  return (
    <aside className="w-64 bg-sidebar text-sidebar-foreground border-r border-sidebar-border flex flex-col p-4">
      
      {/* Logo / Título */}
      <div className="text-2xl font-bold mb-8 px-2 flex items-center gap-2">
        <span className="text-primary font-black tracking-tighter">UBS</span>
        <span className="font-light">Watchdog</span>
      </div>

      {/* Menu */}
      <nav className="flex flex-col gap-1 flex-1">
        <NavLink
          to="/dashboard"
          className={({ isActive }) =>
            `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors
             ${isActive 
               ? "bg-sidebar-accent text-sidebar-accent-foreground shadow-sm" 
               : "hover:bg-sidebar-accent/50 text-sidebar-foreground/80 hover:text-sidebar-foreground"}`
          }
        >
          <LayoutDashboard size={18} />
          Dashboard
        </NavLink>

      <NavLink
          to="/clients"
          className={({ isActive }) =>
            `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors
             ${isActive 
               ? "bg-sidebar-accent text-sidebar-accent-foreground shadow-sm" 
               : "hover:bg-sidebar-accent/50 text-sidebar-foreground/80 hover:text-sidebar-foreground"}`
          }
        >
          <Users size={18} />
          Clients
        </NavLink>

        <NavLink
          to="/transactions"
          className={({ isActive }) =>
            `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors
             ${isActive 
               ? "bg-sidebar-accent text-sidebar-accent-foreground shadow-sm" 
               : "hover:bg-sidebar-accent/50 text-sidebar-foreground/80 hover:text-sidebar-foreground"}`
          }
        >
          <ArrowLeftRight size={18} />
          Transactions
        </NavLink>

    <NavLink
          to="/alerts"
          className={({ isActive }) =>
            `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors
             ${isActive 
               ? "bg-sidebar-accent text-sidebar-accent-foreground shadow-sm" 
               : "hover:bg-sidebar-accent/50 text-sidebar-foreground/80 hover:text-sidebar-foreground"}`
          }
        >
          <AlertCircle size={18} />
          Alerts
        </NavLink>

  
        <NavLink
          to="/reports"
          className={({ isActive }) =>
            `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors
             ${isActive 
               ? "bg-sidebar-accent text-sidebar-accent-foreground shadow-sm" 
               : "hover:bg-sidebar-accent/50 text-sidebar-foreground/80 hover:text-sidebar-foreground"}`
          }
        >
          <FileBarChart size={18} />
          Reports
        </NavLink>

        
        <div className="pt-2 mt-auto border-t border-sidebar-border">
          <NavLink
            to="/admin"
            className={({ isActive }) =>
              `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors
               ${isActive 
                 ? "bg-sidebar-accent text-sidebar-accent-foreground shadow-sm" 
                 : "hover:bg-sidebar-accent/50 text-sidebar-foreground/80 hover:text-sidebar-foreground"}`
            }
          >
            <Settings size={18} />
            Admin
          </NavLink>
          </div>
      </nav>
    </aside>
  );
}