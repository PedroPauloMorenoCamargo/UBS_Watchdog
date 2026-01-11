import type { RiskLevelApi } from "../alert";
import type{ KycStatusApi } from "../kycstatus";

export interface ClientResponseDto {
  id: string;
  legalType: string;
  name: string;
  contactNumber: string;
  addressJson: unknown;
  countryCode: string;
  riskLevel: RiskLevelApi;
  kycStatus: KycStatusApi;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface PagedClientsResponseDto {
  items: ClientResponseDto[];
  page: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
