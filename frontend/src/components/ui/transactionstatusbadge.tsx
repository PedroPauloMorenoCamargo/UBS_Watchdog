import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { TransactionStatus } from "@/types/transactionstatus";

interface TransactionStatusBadgeProps {
  status: TransactionStatus;
}

export function TransactionStatusBadge({ status }: TransactionStatusBadgeProps) {
  const styles = {
    Approved: "bg-green-100 text-green-700 border-green-200",
    Pending: "bg-yellow-100 text-yellow-700 border-yellow-200",
  };

  return (
    <Badge
      variant="outline"
      className={cn(
        "rounded-sm text-xs font-medium capitalize",
        styles[status],
      )}
    >
      {status}
    </Badge>
  );
}
