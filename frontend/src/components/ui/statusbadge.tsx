import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { Status } from "@/types/status";

interface StatusBadgeProps {
  status: Status;
}

function capitalize(value: string) {
  return value.charAt(0).toUpperCase() + value.slice(1).toLowerCase();
}

export function StatusBadge({ status }: StatusBadgeProps) {
  const styles = {
    New: "bg-blue-100 text-blue-700 border-blue-200",
    Analysis: "bg-purple-100 text-purple-700 border-purple-200",
    Resolved: "bg-green-100 text-green-700 border-green-200",
  };
  
  const normalizedStatus = capitalize(status) as keyof typeof styles;

  return (
    <Badge
      variant="outline"
      className={cn(
        "rounded-sm text-xs font-medium capitalize",
        styles[normalizedStatus],
      )}
    >
      {normalizedStatus}
    </Badge>
  );
}
