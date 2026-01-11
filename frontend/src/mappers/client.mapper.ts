// src/mappers/client.mapper.ts
import type { ClientResponseDto } from "@/types/Clients/client";
import type { ClientTableRow } from "@/models/client";
import { mapRiskLevel } from "./riskLevel.mapper";
import { mapKycStatus } from "./kycStatus.mapper";

export function mapClientToTableRow(
  dto: ClientResponseDto
): ClientTableRow {
  return {
    id: dto.id,
    name: dto.name,
    country: dto.countryCode,
    risk: mapRiskLevel(dto.riskLevel),
    kyc: mapKycStatus(dto.kycStatus),
    alerts: 0, //TODO : Receber do backend
    balance: 0, //TODO: Receber do backend
    lastActivity: dto.updatedAtUtc, //TODO: Receber do backend
  };
}
