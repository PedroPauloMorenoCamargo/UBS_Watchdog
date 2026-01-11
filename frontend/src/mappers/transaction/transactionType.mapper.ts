export function mapTransactionType(type: number): string {
  switch (type) {
    case 0:
      return "Debit";
    case 1:
      return "Credit";
    case 2:
      return "Transfer";
    default:
      return "Unknown";
  }
}
