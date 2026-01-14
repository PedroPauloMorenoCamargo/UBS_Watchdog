import { useEffect, useState, useCallback } from "react";

interface UseApiOptions<T> {
  fetcher: () => Promise<T>;
  enabled?: boolean;
  deps?: unknown[];
}

export function useApi<T>({
  fetcher,
  enabled = true,
  deps = [],
}: UseApiOptions<T>) {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(enabled);
  const [error, setError] = useState<string | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  const refetch = useCallback(() => {
    setRefreshKey((prev) => prev + 1);
  }, []);

  const fetch = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const res = await fetcher();
      setData(res);
    } catch (err) {
      console.error(err);
      setError("Failed to load data");
    } finally {
      setLoading(false);
    }
  }, [fetcher]);

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
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fetcher, enabled, refreshKey, ...deps]);

  return { data, loading, error, refetch: fetch };
}
