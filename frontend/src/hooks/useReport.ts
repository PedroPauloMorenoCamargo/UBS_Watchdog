import { useState, useEffect } from "react";
import type { ClientResponseDto } from "@/types/Clients/client";
import type { ClientReportDto } from "@/types/Reports/report";
import { fetchClientReport } from "@/services/reports.service";

export function useReports(clients: ClientResponseDto[]) {
  const [reports, setReports] = useState<ClientReportDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (clients.length === 0) return;

    let isCancelled = false;

    const loadReports = async () => {
      setIsLoading(true);
      try {
        const results = await Promise.all(clients.map(c => fetchClientReport(c.id)));
        if (!isCancelled) setReports(results.flat()); // ⚡ <--- flatten here
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
  }, [clients]);

  return { reports, isLoading };
}
