import { api } from "@/lib/api";
import type { PagedCasesResponseDto } from "@/types/Cases/cases";
export function fetchCases() {
  return api
    .get<PagedCasesResponseDto>("api/cases")
    .then((res) => res.data);
}
