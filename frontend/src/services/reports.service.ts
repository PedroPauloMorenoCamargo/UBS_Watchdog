import { api } from "@/lib/api";
import type { ClientReportDto, ClientDetailDto } from "@/types/Reports/report";

export async function fetchClientReport(clientId: string): Promise<ClientReportDto> {
  const response = await api.get(`api/reports/client/${clientId}`);
  return response.data;
}

export async function fetchClientDetail(clientId: string): Promise<ClientDetailDto> {
  const response = await api.get(`api/clients/${clientId}`);
  return response.data;
}
