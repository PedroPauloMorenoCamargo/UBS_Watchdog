import type { ReactNode } from "react";
import {
  AlertTriangle,
  Clock,
  CheckCircle,
} from "lucide-react";
import { cn } from "@/lib/utils";

type Variant = "destructive"| "high" | "medium" | "low";

interface StatCardProps {
  title: string;
  value: string | number;
  variant: Variant;
}

const variantConfig: Record<
  Variant,
  {
    bg: string;
    iconBg: string;
    iconColor: string;
    Icon: ReactNode;
  }
> = {
  destructive: {
    bg: "bg-red-400/20",
    iconBg: "bg-red-500/40",
    iconColor: "text-red-700",
    Icon: <AlertTriangle className="h-6 w-6" />,
  },
  high: {
    bg: "bg-orange-400/20",
    iconBg: "bg-orange-500/40",
    iconColor: "text-orange-700",
    Icon: <AlertTriangle className="h-6 w-6" />,
  },
  medium: {
    bg: "bg-yellow-400/20",
    iconBg: "bg-yellow-500/40",
    iconColor: "text-yellow-700",
    Icon: <Clock className="h-6 w-6" />,
  },
  low: {
    bg: "bg-green-400/20",
    iconBg: "bg-green-500/40",
    iconColor: "text-green-700",
    Icon: <CheckCircle className="h-6 w-6" />,
  },
};

export function StatCard({ title, value, variant }: StatCardProps) {
  const styles = variantConfig[variant];

  return (
    <div
      className={cn(
        "rounded-xl p-4 shadow-lg flex items-center justify-left transition-all hover:shadow-xl",
        styles.bg
      )}
    >
      <div className="flex items-center gap-4">
        {/* Ícone */}
        <div
          className={cn(
            "flex h-12 w-12 items-center justify-center rounded-full",
            styles.iconBg,
            styles.iconColor
          )}
        >
          {styles.Icon}
        </div>

        {/* Texto */}
        <div>
          <h3 className="text-sm font-semibold text-slate-800">
            {title}
          </h3>
          <p className="text-3xl font-bold text-slate-900">
            {value}
          </p>
        </div>
      </div>
    </div>
  );
}
