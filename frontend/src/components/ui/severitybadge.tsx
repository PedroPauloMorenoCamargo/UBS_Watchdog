import { Badge } from "@/components/ui/badge";
import type { Severity } from "@/types/alert";
import { cn } from "@/lib/utils";

interface SeverityBadgeProps {
  severity: Severity;
  className?: string;
}

export function SeverityBadge({ severity,className }: SeverityBadgeProps) {
  const styles = {
    High: "bg-red-100 text-red-700 border-red-200",
    Medium: "bg-yellow-100 text-yellow-700 border-yellow-200",
    Low: "bg-green-100 text-green-700 border-green-200",
  };

  return (
    <Badge
      variant="outline"
      className={cn(
        "border text-xs font-medium capitalize",
        styles[severity],
        className,
      )}
    >
      {severity}
    </Badge>
  );
}
