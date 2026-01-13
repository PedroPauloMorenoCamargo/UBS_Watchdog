import { api } from "@/lib/api";

export interface CountryDto {
  code: string;
  name: string;
  riskLevel: number;
}

export async function fetchCountries(): Promise<CountryDto[]> {
  const response = await api.get("api/countries");
  return response.data;
}

export function fetchCountries(): Promise<CountryDto[]> {
  return api
    .get<CountryDto[]>("api/countries")
    .then((res) => res.data);
}
