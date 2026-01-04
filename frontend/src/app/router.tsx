import { createBrowserRouter } from "react-router-dom";
import { LoginPage } from "@/pages/LoginPage";
import { Dashboard } from "@/pages/Dashboard";
import { AppLayout } from "@/layout/AppLayout";
import { requireAuth } from "@/middlewares/authMiddleware";
import { requireRole } from "@/middlewares/roleMiddleware";
import { AlertsPage } from "@/pages/AlertsPage";

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
      },
      {
        path: "/clients",
        element: <div>Clients Page</div>,
      },
      {
        path: "/transactions",
        element: <div>Transactions Page</div>,
      },
      {
        path: "/reports",
        element: <div>Reports Page</div>,
      },
      {
        path: "/alerts",
        element: <AlertsPage />,
      },
      {
        path: "/admin",
        loader: requireRole(["admin"]),
        element: <div>Admin Page</div>,
      },
      {
        path: "/configurations",
        element: <div>Configurations Page</div>,
      },
    ],
  },
]);
