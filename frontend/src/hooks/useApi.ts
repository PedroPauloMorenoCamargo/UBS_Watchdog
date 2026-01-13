import { useEffect, useState } from "react";

interface UseApiOptions<T> {
  fetcher: () => Promise<T>;
  enabled?: boolean;
}

export function useApi<T>({
  fetcher,
  enabled = true,
}: UseApiOptions<T>) {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(enabled);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!enabled) return;

    let mounted = true;

    setLoading(true);
    setError(null);

    fetcher()
      .then((res) => {
        if (mounted) setData(res);
      })
      .catch((err) => {
        console.error(err);
        if (mounted) setError("Failed to load data");
      })
      .finally(() => {
        if (mounted) setLoading(false);
      });

    return () => {
      mounted = false;
    };
  }, [fetcher, enabled]);

  return { data, loading, error };
}
