export const COUNTRIES = ["BR", "US", "AR"] as const;

export type Countries = typeof COUNTRIES[number];
export type CountriesFilter = Countries | "all";
