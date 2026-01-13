import { api } from "@/lib/api";
import type { PagedClientsResponseDto } from "@/types/Clients/client";

export function fetchClients() {
  return api
    .get<PagedClientsResponseDto>("api/clients")
    .then((res) => res.data);
}
