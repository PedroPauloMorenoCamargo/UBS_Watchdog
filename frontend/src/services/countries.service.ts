import { api } from "@/lib/api";
import type { CountriesResponseDto } from "@/types/Countries/countries";
export function fetchCountries() {
  return api
    .get<CountriesResponseDto[]>("api/countries")
    .then((res) => res.data);
}
