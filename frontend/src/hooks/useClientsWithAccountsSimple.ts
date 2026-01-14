import { useEffect, useState } from "react";
import { api } from "@/lib/api";

export interface ClientSimple {
  id: string;
  name: string;
}

export function useClientsWithAccountsSimple() {
  const [clients, setClients] = useState<ClientSimple[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    api.get("/api/clients", { params: { "Page.Page": 1, "Page.PageSize": 100 } })
      .then((res) => {
        setClients(res.data.items);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  return { clients, loading, error };
}
