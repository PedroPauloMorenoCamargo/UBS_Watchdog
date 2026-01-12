export const COUNTRY_MAP: Record<string, string> = {
  US: "United States",
  CH: "Switzerland",
  GB: "United Kingdom",
  SG: "Singapore",
  DE: "Germany",
};

export function mapCountryCode(code: string | null| undefined): string {
  switch (code) {
    case "US": return "United States";
    case "CH": return "Switzerland";
    case "GB": return "United Kingdom";
    case "SG": return "Singapore";
    case "DE": return "Germany";
    default: return "Others";
  }
}