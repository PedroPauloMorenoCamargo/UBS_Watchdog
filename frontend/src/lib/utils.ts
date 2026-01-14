import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function formatDateTime(
  isoDate: string,
  locale = "pt-BR"
): string {
  const date = new Date(isoDate);

  return new Intl.DateTimeFormat(locale, {
    dateStyle: "short",
    timeStyle: "short",
  }).format(date);
}

export function formatCurrencyCompact(
  value: number | null | undefined,
  decimals: number = 2
): string {
  if (value == null || isNaN(value)) return "0";

  const abs = Math.abs(value);

  if (abs >= 1_000_000_000) return (value / 1_000_000_000).toFixed(decimals).replace(/\.0+$/, "") + "B";
  if (abs >= 1_000_000) return (value / 1_000_000).toFixed(decimals).replace(/\.0+$/, "") + "M";
  if (abs >= 1_000) return (value / 1_000).toFixed(decimals).replace(/\.0+$/, "") + "K";

  return value.toFixed(decimals).replace(/\.0+$/, "");
}




