import { Badge } from "@/components/ui/badge";

interface SeverityBadgeProps {
  severity: "High" | "Medium" | "Low";
}

export function SeverityBadge({ severity }: SeverityBadgeProps) {
  const styles = {
    High: "bg-red-100 text-red-700 border-red-200",
    Medium: "bg-yellow-100 text-yellow-700 border-yellow-200",
    Low: "bg-green-100 text-green-700 border-green-200",
  };

  return (
    <Badge
      variant="outline"
      className={styles[severity]}
    >
      {severity}
    </Badge>
  );
}
