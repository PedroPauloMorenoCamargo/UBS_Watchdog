import { useState } from "react";
import { importClientsCsv } from "@/services/clients.service";

export function useImportClientsCsv(onSuccess?: () => void) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit(file: File) {
    try {
      setLoading(true);
      setError(null);

      await importClientsCsv(file);
      onSuccess?.();

    } catch (e) {
      console.error(e);
      setError("Failed to import CSV");
      
    } finally {
      setLoading(false);
    }
  }

  return { submit, loading, error };
}
