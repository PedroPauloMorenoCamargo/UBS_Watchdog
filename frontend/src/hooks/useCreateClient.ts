import { useState } from "react";
import { createClient } from "@/services/clients.service";
import type { CreateClientDto } from "@/types/clients/client";

interface CreateClientResult {
  success: boolean;
  error?: string;
}

export function useCreateClient() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit(
    payload: CreateClientDto
  ): Promise<CreateClientResult> {
    try {
      setLoading(true);
      setError(null);

      await createClient(payload);

      return { success: true };
    } catch (err: any) {
      const message =
        err?.response?.data?.message ??
        err?.message ??
        "Erro ao criar cliente";

      setError(message);

      return {
        success: false,
        error: message,
      };
    } finally {
      setLoading(false);
    }
  }

  return {
    submit,
    loading,
    error,
  };
}
