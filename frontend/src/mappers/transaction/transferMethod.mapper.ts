export function mapTransferMethod(method?: number | null): string {
  if (method === null || method === undefined) return "-";

  switch (method) {
    case 0:
      return "Wire";
    case 1:
      return "ACH";
    case 2:
      return "Crypto";
    default:
      return "Other";
  }
}
