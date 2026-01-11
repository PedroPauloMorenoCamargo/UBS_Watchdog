export type RiskLevelApi = 0 | 1 | 2;
export type KycStatusApi = 0 | 1 | 2 | 3;

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
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
