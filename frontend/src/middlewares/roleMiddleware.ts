import { redirect } from "react-router-dom";
import { useAuthStore } from "@/store/auth";

export function requireRole(allowedRoles: string[]) {
  return () => {
    const { user } = useAuthStore.getState();

    // TODO: Implementar sistema de roles/permissões
    // const role = user?.role;
    const role = user?.email; // temporariamente usando email

    if (!role || !allowedRoles.includes(role)) {
      throw redirect("/dashboard");
    }

    return null;
  };
}
