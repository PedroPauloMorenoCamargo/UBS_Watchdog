import { api } from "@/lib/api";
import type { 
  PagedCasesResponseDto, 
  CaseDetailedResponseDto, 
  UpdateCaseRequest, 
  CaseResponseDto 
} from "@/types/Cases/cases";

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

export function getCaseDetails(id: string) {
  return api
    .get<CaseDetailedResponseDto>(`api/cases/${id}`)
    .then((res) => res.data);
}

export function assignToMe(id: string) {
  return api
    .post<CaseResponseDto>(`api/cases/${id}/assign-to-me`)
    .then((res) => res.data);
}

export function updateCase(id: string, data: UpdateCaseRequest) {
  return api
    .patch<CaseResponseDto>(`api/cases/${id}`, data)
    .then((res) => res.data);
}
