import { api } from "@/lib/api";
import type {
  PagedClientsResponseDto,
  CreateClientDto,
  ImportCsvResponseDto,
} from "@/types/Clients/client";

export interface FetchClientsParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  riskLevel?: string;
  countryCode?: string;
  kycStatus?: string;
}

export function fetchClients(params?: FetchClientsParams) {
  return api
    .get<PagedClientsResponseDto>("api/clients", {
      params: {
        "Page.Page": params?.page ?? 1,
        "Page.PageSize": params?.pageSize ?? 20,
        SearchTerm: params?.searchTerm || undefined,
        RiskLevel: params?.riskLevel || undefined,
        CountryCode: params?.countryCode || undefined,
        KycStatus: params?.kycStatus || undefined,
      },
    })
    .then((res) => res.data);
}

export function createClient(payload: CreateClientDto) {
  return api.post("api/clients", payload);
}

export function importClientsCsv(
  file: File
): Promise<ImportCsvResponseDto> {
  const formData = new FormData();
  formData.append("file", file);

  return api
    .post<ImportCsvResponseDto>("api/clients/import", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    })
    .then((res) => res.data);
}
