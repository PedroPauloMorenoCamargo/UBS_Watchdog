import type { RiskLevelApi } from "../alert";
import type{ KycStatusApi } from "../kycstatus";
import type { LegalTypeApi } from "../legaltypeapi";

export interface ClientResponseDto {
  id: string;
  legalType: LegalTypeApi;
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


export interface CreateClientDto {
  legalType: LegalTypeApi;
  name: string;
  contactNumber?: string;
  addressJson?: string;
  countryCode: string;
  initialRiskLevel: RiskLevelApi;
  kycStatus: KycStatusApi;
}

export interface ImportCsvResponseDto {
  importedCount: number;
  failedCount: number;
  errors?: { row: number; message: string }[];
}
