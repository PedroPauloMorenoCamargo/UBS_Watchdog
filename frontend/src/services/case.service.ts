import { api } from "@/lib/api";
import type { PagedCasesResponseDto } from "@/types/Cases/cases";

export interface FetchCasesParams {
  page?: number;
  pageSize?: number;
}

export function fetchCases(params?: FetchCasesParams) {
  return api
    .get<PagedCasesResponseDto>("api/cases", {
      params: {
        page: params?.page ?? 1,
        pageSize: params?.pageSize ?? 20,
      },
    })
    .then((res) => res.data);
}
