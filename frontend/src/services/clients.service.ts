import { api } from "@/lib/api";
import type {
  PagedClientsResponseDto,
  CreateClientDto,
  ImportCsvResponseDto,
} from "@/types/Clients/client";

export function fetchClients() {
  return api
    .get<PagedClientsResponseDto>("api/clients")
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
