import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

interface KYCStatusBadgeProps {
  kyc:  "Expired" | "Pending" | "Verified" | "Rejected";
}

export function KYCStatusBadge({ kyc }: KYCStatusBadgeProps) {
  const styles = {
    Expired: "bg-grey-100 text-grey-700 border-grey-200",
    Pending: "bg-yellow-100 text-yellow-700 border-yellow-200",
    Verified: "bg-green-100 text-green-700 border-green-200",
    Rejected: "bg-red-100 text-red-700 border-red-200",
  };

  return (
    <Badge
      variant="outline"
      className={cn(
        "rounded-sm text-xs font-medium capitalize",
        styles[kyc],
      )}
    >
      {kyc}
    </Badge>
  );
}
