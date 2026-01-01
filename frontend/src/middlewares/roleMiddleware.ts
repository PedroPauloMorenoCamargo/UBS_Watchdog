import { redirect } from "react-router-dom";
import { useAuthStore } from "@/store/auth";

export function requireRole(allowedRoles: string[]) {
  return () => {
    const { user } = useAuthStore.getState();

    const role = user?.username; // exemplo
    // ideal: user.role

    if (!role || !allowedRoles.includes(role)) {
      throw redirect("/dashboard");
    }

    return null;
  };
}
