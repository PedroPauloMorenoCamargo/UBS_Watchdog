import { useState, useEffect } from "react";
import type { ClientResponseDto } from "@/types/Clients/client";
import type { ClientReportDto } from "@/types/Reports/report";
import { fetchClientReport } from "@/services/reports.service";

export function useReports(
  clients: ClientResponseDto[],
  currentPage: number,
  pageSize: number,
  options?: { clientsArePaged?: boolean }
) {
  const [reports, setReports] = useState<ClientReportDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  // Calcula total de páginas baseado na lista completa de clients
  const totalPages = Math.ceil(clients.length / pageSize);

  useEffect(() => {
    if (!clients || clients.length === 0) {
      setReports([]);
      return;
    }

    let isCancelled = false;

    const loadReports = async () => {
      setIsLoading(true);
      try {
        // Se os `clients` já representam uma página trazida do servidor,
        // usa-os diretamente; caso contrário, faz a paginação no cliente.
        let clientsPage: ClientResponseDto[];
        if (options?.clientsArePaged) {
          clientsPage = clients;
        } else {
          const start = (currentPage - 1) * pageSize;
          const end = start + pageSize;
          clientsPage = clients.slice(start, end);
        }

        // Busca relatórios apenas desses clients
        const results = await Promise.all(
          clientsPage.map(c => fetchClientReport(c.id))
        );

        if (!isCancelled) {
          setReports(results.flat());
        }
      } catch (err) {
        console.error("Failed to fetch client reports:", err);
      } finally {
        if (!isCancelled) setIsLoading(false);
      }
    };

    loadReports();

    return () => {
      isCancelled = true;
    };
  }, [clients, currentPage, pageSize]);

  return { reports, isLoading, totalPages };
}
