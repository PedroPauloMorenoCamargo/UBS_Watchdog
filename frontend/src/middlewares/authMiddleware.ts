import { redirect } from "react-router-dom";
import { useAuthStore } from "@/store/auth";

/**
 * Middleware: exige usuário autenticado
 */
export function requireAuth() {
  const { isAuthenticated } = useAuthStore.getState();

  if (!isAuthenticated) {
    throw redirect("/");
  }

  return null;
}
