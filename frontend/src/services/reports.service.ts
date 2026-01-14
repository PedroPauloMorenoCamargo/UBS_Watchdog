import { api } from "@/lib/api";
import type { ClientReportDto, ClientDetailDto } from "@/types/Reports/report";
import type { ClientResponseDto } from "@/types/Clients/client";

export async function fetchClientReport(clientId: string): Promise<ClientReportDto> {
  const response = await api.get(`api/reports/client/${clientId}`);
  return response.data;
}

export async function fetchClientDetail(clientId: string): Promise<ClientDetailDto> {
  const response = await api.get(`api/clients/${clientId}`);
  return response.data;
}

export async function fetchAllClientsReports(clients: ClientResponseDto[]): Promise<ClientReportDto[]> {
  // Criar um array de promises para cada client
  const reportPromises = clients.map(client => fetchClientReport(client.id));

  // Espera todas as requisições terminarem
  const reports = await Promise.all(reportPromises);

  return reports;
}

export async function exportSystemReportCsv(): Promise<Blob> {
  const response = await api.get("api/reports/system/export/csv", {
    responseType: "blob",
  });
  return response.data;
}
