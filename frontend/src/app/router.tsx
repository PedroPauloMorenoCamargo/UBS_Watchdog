import { createBrowserRouter } from "react-router-dom";
import { LoginPage } from "@/pages/LoginPage";
import { Dashboard } from "@/pages/Dashboard";
import { AppLayout } from "@/layout/AppLayout";
import { requireAuth } from "@/middlewares/authMiddleware";
import { requireRole } from "@/middlewares/roleMiddleware";
import { AlertsPage } from "@/pages/AlertsPage";
import { TransactionsPage } from "@/pages/TransactionsPage";
import { ClientsPage } from "@/pages/ClientsPage";

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
        element: <div>Reports Page</div>,
        handle: {
          title: "Reports",
        },
      },
      {
        path: "/admin",
        loader: requireRole(["admin"]),
        element: <div>Admin Page</div>,
        handle: {
          title: "Admin",
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
