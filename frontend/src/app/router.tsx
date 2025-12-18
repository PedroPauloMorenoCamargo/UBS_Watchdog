import { createBrowserRouter } from "react-router-dom";

//Layout
import App from "@/App";
import { Home } from "@/pages/Home";
import { LoginPage } from "@/pages/LoginPage";
import { DashboardPage } from "@/pages/DashboardPage";

// All routes are written here 
// the element <App /> below encapsulates all the pages,
// this way all of them follows same pattern, with header and footer.

export const router = createBrowserRouter([
  { 
    path: "/", 
    element: <App/>,
    children: [
      {
        index:true,
        element: <Home />,
      },
      // futuras rotas:
      {
        path: "login",
        element: <LoginPage />,
      },
      { 
        path: "dashboard", 
        element: <DashboardPage />,
      },
    ]
  }
]);
