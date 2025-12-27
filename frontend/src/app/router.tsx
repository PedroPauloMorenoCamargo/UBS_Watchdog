import { createBrowserRouter } from "react-router-dom";
import { LoginPage } from "@/pages/LoginPage";
import { Dashboard } from "@/pages/Dashboard";
import { AppLayout } from "@/layout/AppLayout";
import { requireAuth } from "@/middlewares/authMiddleware";
import { requireRole } from "@/middlewares/roleMiddleware";

export const router = createBrowserRouter([
  { 
    path: "/", 
    element: <LoginPage /> 
  },
  {
    element: <AppLayout />, //Layout para encapsular as paginas que terão Navbar depois de feito login
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
        element: <div>Alerts Page</div>,
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
