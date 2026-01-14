import { useEffect, useState } from "react";
import { api } from "@/lib/api";

export interface AccountIdentifier {
  id: string;
  identifierType: number;
  identifierValue: string;
}

export function useClientAccountIdentifiers(accountId?: string) {
  const [identifiers, setIdentifiers] = useState<AccountIdentifier[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!accountId) {
      setIdentifiers([]);
      return;
    }
    setLoading(true);
    api.get(`/api/accounts/${accountId}/identifiers`)
      .then((res) => setIdentifiers(res.data))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [accountId]);

  return { identifiers, loading, error };
}
