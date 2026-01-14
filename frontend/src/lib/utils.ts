import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function formatCurrency(
  value: number,
  currency: string = "USD"
): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: currency,
  }).format(value);
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
  value: number,
  currency: string = "USD",
  decimals: number = 1
): string {
  const abs = Math.abs(value);

  if (abs >= 1_000_000_000) {
    return `${(value / 1_000_000_000).toFixed(decimals)}B`;
  }

  if (abs >= 1_000_000) {
    return `${(value / 1_000_000).toFixed(decimals)}M`;
  }

  // fallback normal
  return value.toLocaleString("en-US", {
    // style: "currency",
    currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}
