import { api } from "@/lib/api";

export async function exportClientReportCsv(clientId: string): Promise<Blob> {
  const response = await api.get(`api/reports/client/${clientId}/export/csv`, {
    responseType: "blob",
  });
  return response.data;
}
