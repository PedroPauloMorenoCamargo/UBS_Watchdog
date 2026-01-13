import { api } from "@/lib/api";
import type { PagedCasesResponseDto } from "@/types/Cases/cases";
export function fetchClients() {
  return api
    .get<PagedCasesResponseDto>("api/clients")
    .then((res) => res.data);
}
