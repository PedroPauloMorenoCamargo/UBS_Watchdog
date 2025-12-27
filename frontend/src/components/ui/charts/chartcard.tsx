import type { ReactNode } from "react";
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
} from "@/components/ui/card";

interface ChartCardProps {
  title: string;
  description?: string;
  height?: string; // ex: "h-64", "h-80"
  children: ReactNode;
}

export function ChartCard({
  title,
  description,
  height = "h-64",
  children,
}: ChartCardProps) {
  return (
    <Card className="rounded-xl shadow-sm">
      <CardHeader>
        <CardTitle className="text-lg">{title}</CardTitle>
        {description && (
          <CardDescription>{description}</CardDescription>
        )}
      </CardHeader>

      <CardContent>
        <div className={height}>
          {children}
        </div>
      </CardContent>
    </Card>
  );
}