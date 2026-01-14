import type { TransactionResponseDto } from "@/types/Transactions/transaction";
import type { TransactionRow } from "@/models/transaction";
import { mapTransactionType } from "./transactionType.mapper";
import { mapTransferMethod } from "./transferMethod.mapper";



export function mapTransactionToRow(dto: TransactionResponseDto): TransactionRow {
  const occurred = new Date(dto.occurredAtUtc);

  return {
    id: dto.id,
    date: occurred.toLocaleString("pt-BR"), // exibição
    rawTimestamp: occurred.getTime(),       // para filtro
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

