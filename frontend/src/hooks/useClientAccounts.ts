import { useEffect, useState } from "react";
import { api } from "@/lib/api";

export interface Account {
  id: string;
  accountIdentifier: string;
  currencyCode: string;
}

export function useClientAccounts(clientId?: string) {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!clientId) {
      setAccounts([]);
      return;
    }
    setLoading(true);
    api.get(`/api/clients/${clientId}/accounts`)
      .then((res) => setAccounts(res.data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [clientId]);

  return { accounts, loading, error };
}
