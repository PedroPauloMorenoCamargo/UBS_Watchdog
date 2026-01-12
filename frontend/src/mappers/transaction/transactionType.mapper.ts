export function mapTransactionType(type: number): string {
  switch (type) {
    case 0:
      return "Deposit";
    case 1:
      return "Withdrawal";
    case 2:
      return "Wire";
    default:
      return "Unknown";
  }
}
