import { useState, useEffect } from "react";
import { fetchCountries, type CountryDto } from "@/services/countries.service";

export function useCountries() {
  const [countries, setCountries] = useState<CountryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadCountries() {
      try {
        setLoading(true);
        const data = await fetchCountries();
        setCountries(data);

      } catch (err: any) {
        console.error(err);
        setError("Error loading countries");

      } finally {
        setLoading(false);
      }
    }

    loadCountries();
  }, []);

  return { countries, loading, error };
}
