import { Badge } from "@/components/ui/badge";
import type { Severity } from "@/types/alert";
import { cn } from "@/lib/utils";

interface SeverityBadgeProps {
  severity: Severity;

}

function capitalize(value: string) {
  return value.charAt(0).toUpperCase() + value.slice(1).toLowerCase();
}

export function SeverityBadge({ severity }: SeverityBadgeProps) {
  const styles = {
    High: "bg-red-100 text-red-700 border-red-200",
    Medium: "bg-yellow-100 text-yellow-700 border-yellow-200",
    Low: "bg-green-100 text-green-700 border-green-200",
  };
  
  const normalizedSeverity = capitalize(severity) as keyof typeof styles;

  return (
    <Badge
      variant="outline"
      className={cn(
        "rounded-sm text-xs font-medium capitalize",
        styles[normalizedSeverity],
      )}
    >
      {normalizedSeverity}
    </Badge>
  );
}
