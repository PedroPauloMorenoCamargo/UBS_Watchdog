import type { TransactionResponseDto } from "@/types/Transactions/transaction";
import type { TransactionRow } from "@/models/transaction";
import { mapTransactionType } from "./transactionType.mapper";
import { mapTransferMethod } from "./transferMethod.mapper";

export function mapTransactionToRow(
  dto: TransactionResponseDto
): TransactionRow {
  return {
    id: dto.id,
    date: new Date(dto.occurredAtUtc).toLocaleString("pt-BR"),
    amount: `${dto.amount.toFixed(2)}`,
    rawAmount: dto.amount,
    type: mapTransactionType(dto.type),
    method: mapTransferMethod(dto.transferMethod),
    clientId: dto.clientId,
    counterIdentifier: dto.cpIdentifier ?? "-",
    country: dto.cpCountryCode ?? "-",
    cpName: dto.cpName ?? "-",
    currencyCode: dto.currencyCode
  };
}
