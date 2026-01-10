import { createBrowserRouter } from "react-router-dom";
import { LoginPage } from "@/pages/LoginPage";
import { Dashboard } from "@/pages/Dashboard";
import { AppLayout } from "@/layout/AppLayout";
import { requireAuth } from "@/middlewares/authMiddleware";
import { requireRole } from "@/middlewares/roleMiddleware";
import { AlertsPage } from "@/pages/AlertsPage";
import { TransactionsPage } from "@/pages/TransactionsPage";
import { ClientsPage } from "@/pages/ClientsPage";
import { ReportsPage } from "@/pages/ReportsPage";
import { AdminPage } from "@/pages/AdminPage";

export const router = createBrowserRouter([
  { 
    path: "/", 
    element: <LoginPage /> 
  },
  {
    element: <AppLayout />, 
    loader: requireAuth,
    children: [
      {
        path: "/dashboard",
        element: <Dashboard />,
        handle: {
          title: "Dashboard",
        },
      },
      {
        path: "/alerts",
        element: <AlertsPage />,
        handle: {
          title: "Alerts",
        },
      },
      {
        path: "/transactions",
        element: <TransactionsPage />,
        handle: {
          title: "Transactions",
        },
      },
      {
        path: "/clients",
        element: <ClientsPage />,
        handle: {
          title: "Clients",
        },
      },
      {
        path: "/reports",
        element: <ReportsPage />,
        handle: {
          title: "Compliance Reports",
        },
      },
      {
        path: "/admin",
        loader: requireRole(["analyst@ubs.com"]), //TODO: Role check
        element: <AdminPage />,
        handle: {
          title: "Admin Console",
        },
      },
      {
        path: "/configurations",
        element: <div>Configurations Page</div>,
        handle: {
          title: "Configurations",
        },
      },
    ],
  },
]);
