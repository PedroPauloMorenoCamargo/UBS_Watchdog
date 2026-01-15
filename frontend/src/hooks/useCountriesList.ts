import { useEffect, useState } from "react";
import { fetchCountries } from "@/services/countries.service";
import type { CountriesResponseDto } from "@/types/Countries/countries";

export function useCountriesList() {
  const [countries, setCountries] = useState<CountriesResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    fetchCountries()
      .then((data) => {
        setCountries(data);
        setError(null);
      })
      .catch(() => {
        setError("Erro ao carregar países");
        setCountries([]);
      })
      .finally(() => setLoading(false));
  }, []);

  return { countries, loading, error };
}
