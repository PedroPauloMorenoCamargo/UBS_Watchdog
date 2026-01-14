import { useState } from "react";
import { createClient } from "@/services/clients.service";
import type { CreateClientDto } from "@/types/Clients/client";

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
      let message = "Error creating client";
      
      if (err?.response?.data) {
        const data = err.response.data;

        if (data.errors && typeof data.errors === "object") {
          const errorMessages = Object.values(data.errors)
            .flat()
            .filter(Boolean);
          if (errorMessages.length > 0) {
            message = errorMessages.join(". ");
          }
        } else if (data.title) {
          message = data.title;
        } else if (data.message) {
          message = data.message;
        }
      } else if (err?.message) {
        message = err.message;
      }

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
