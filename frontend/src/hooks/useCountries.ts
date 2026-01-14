import { useMemo } from "react";
import type { CountriesResponseDto } from "@/types/Countries/countries";

export function useCountriesMap(countries: CountriesResponseDto[]) {
  return useMemo(() => {
    if (!countries.length) return null;

    return new Map(countries.map(c => [c.code, c.name]));
  }, [countries]);
}
