import { useState, useEffect } from "react";
import { api } from "@/lib/api";

export interface ClientWithAccounts {
  id: string;
  name: string;
  accounts: Array<{
    id: string;
    accountNumber: string;
  }>;
}

export function useClientsWithAccounts() {
  const [clients, setClients] = useState<ClientWithAccounts[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchClients();
  }, []);

  async function fetchClients() {
    setLoading(true);
    setError(null);

    try {
      // Buscar clientes
      const clientsResponse = await api.get("/api/clients", {
        params: {
          "Page.Page": 1,
          "Page.PageSize": 100, // Buscar todos os clientes
        },
      });

      const clientsList = clientsResponse.data.items;

      // Para cada cliente, buscar suas contas
      const clientsWithAccounts = await Promise.all(
        clientsList.map(async (client: any) => {
          try {
            const accountsResponse = await api.get(`/api/clients/${client.id}/accounts`);
            return {
              id: client.id,
              name: client.name,
              accounts: accountsResponse.data.map((account: any) => ({
                id: account.id,
                accountNumber: account.accountNumber,
              })),
            };
          } catch (err) {
            // Se falhar ao buscar contas, retornar cliente sem contas
            return {
              id: client.id,
              name: client.name,
              accounts: [],
            };
          }
        })
      );

      setClients(clientsWithAccounts);
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || "Failed to fetch clients");
    } finally {
      setLoading(false);
    }
  }

  return { clients, loading, error, refetch: fetchClients };
}
