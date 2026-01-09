export const COUNTRIES = ["Brazil", "USA"] as const;

export type Countries = typeof COUNTRIES[number];
export type CountriesFilter = Countries | "all";
