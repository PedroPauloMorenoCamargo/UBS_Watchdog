import type { CountriesRow } from "@/models/countries";
import type { CountriesResponseDto } from "@/types/Countries/countries";

export function mapCountriesDtoToTableRow(dto: CountriesResponseDto): CountriesRow {
  return {
    code: dto.code,
    name: dto.name,
    riskLevel: dto.riskLevel
  };
}