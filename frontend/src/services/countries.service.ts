import { api } from "@/lib/api";

export interface CountryDto {
  code: string;
  name: string;
}

export function fetchCountries(): Promise<CountryDto[]> {
  return api
    .get<CountryDto[]>("api/countries")
    .then((res) => res.data);
}
