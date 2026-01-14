import { useMemo } from "react";
import type { TransactionResponseDto } from "@/types/Transactions/transaction";
import { mapTransactionType } from "@/mappers/transaction/transactionType.mapper";

import { TRANSACTION_TYPES } from "@/constants/transactionTypes";
import { MONTHS } from "@/constants/months";
import type { CaseDecision, CaseResponseDto } from "@/types/Cases/cases";

import { WEEK_DAYS } from "@/constants/weekdays";
import type { CountriesResponseDto } from "@/types/Countries/countries";
import { useCountriesMap } from "./useCountries";
type Trend = "up" | "down" | "neutral";

interface TransactionsByType {
  name: string;
  value: number;
}

interface TransactionCountry {
  country: string;
  totalAmount: number;
  count: number;
}

export interface WeeklyActivity {
  day: string;
  transactions: number;
  alerts: number;
}

export interface MonthlyVolume {
  month: string;
  volume: number;
}

interface UseTransactionsResult {
  totalTransactionsAmount: number;
  totalTransactionsCount: number;
  transactionTrend: Trend;
  transactionPercentageChange: number | null;
  transactionsByType: TransactionsByType[];
  weeklyActivity: WeeklyActivity[];
  monthlyVolume : MonthlyVolume[];
  transactionsCountry: TransactionCountry[];
}

export function useTransactions(transactions: TransactionResponseDto[], cases: CaseResponseDto[], countries: CountriesResponseDto[]): UseTransactionsResult {
   const now = new Date();

   const countryNameByCode = useCountriesMap(countries);


  const totalTransactionsAmount = useMemo(() => {
    return transactions
      .filter(t => t.baseAmount > 0)
      .reduce((sum, t) => sum + t.baseAmount, 0);
  }, [transactions]);

  const totalTransactionsCount = transactions.length;

  const currentTotal = useMemo(() => {
    const from = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
    return transactions
      .filter(t => new Date(t.occurredAtUtc) >= from)
      .reduce((sum, t) => sum + Math.max(t.amount, 0), 0);
  }, [transactions]);

  const previousTotal = useMemo(() => {
    const from = new Date(now.getTime() - 14 * 24 * 60 * 60 * 1000);
    const to = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);

    return transactions
      .filter(t => {
        const d = new Date(t.occurredAtUtc);
        return d >= from && d < to;
      })
      .reduce((sum, t) => sum + Math.max(t.amount, 0), 0);
  }, [transactions]);

  const transactionTrend: Trend =
    currentTotal > previousTotal
      ? "up"
      : currentTotal < previousTotal
      ? "down"
      : "neutral";

  const transactionPercentageChange =
    previousTotal === 0
      ? null
      : ((currentTotal - previousTotal) / previousTotal) * 100;

  

  const transactionsByType = useMemo<TransactionsByType[]>(() => {
    const map = new Map<string, number>();

    
    TRANSACTION_TYPES.forEach(type => {
      map.set(type, 0);
    });

    transactions.forEach(t => {
      const type = mapTransactionType(t.type);
      if (map.has(type)) {
        map.set(type, (map.get(type) ?? 0) + 1);
      }
    });

    return TRANSACTION_TYPES.map(type => ({
      name: type,
      value: map.get(type) ?? 0,
    }));
  }, [transactions]);

  const weeklyActivity = useMemo<WeeklyActivity[]>(() => {
  
    const data: WeeklyActivity[] = WEEK_DAYS.map(day => ({
      day,
      transactions: 0,
      alerts: 0,
    }));

    const alertsByTxAndDay = new Set<string>();

    cases.forEach(c => {
      const d = new Date(c.openedAtUtc);
      const dayIndex = d.getUTCDay(); 
      alertsByTxAndDay.add(`${c.transactionId}_${dayIndex}`);
    });

    transactions.forEach(t => {
      const d = new Date(t.occurredAtUtc);
      const dayIndex = d.getUTCDay();

      data[dayIndex].transactions += 1;

      if (alertsByTxAndDay.has(`${t.id}_${dayIndex}`)) {
        data[dayIndex].alerts += 1;
      }
    });

    return data;
  }, [transactions, cases]);




  const monthlyVolume = useMemo(() => {
  const volumeByMonth = new Map<number, number>();

  for (let i = 0; i < 12; i++) {
    volumeByMonth.set(i, 0);
  }

  transactions.forEach(t => {
    if (!t.occurredAtUtc) return;

    const date = new Date(t.occurredAtUtc);
    const monthIndex = date.getUTCMonth(); 

    const current = volumeByMonth.get(monthIndex) ?? 0;
    volumeByMonth.set(
      monthIndex,
      current + Math.max(t.baseAmount ?? 0, 0)
    );
  });

  return MONTHS.map((month, index) => ({
    month,
    volume: volumeByMonth.get(index) ?? 0,
  }));
}, [transactions]);

const transactionsCountry = useMemo<TransactionCountry[]>(() => {
  if (!countryNameByCode) return [];

  const map = new Map<string, { totalAmount: number; count: number }>();

  transactions.forEach(t => {
    if (!t.cpCountryCode) return;

    const countryName =
      countryNameByCode.get(t.cpCountryCode) ?? "Others";

    const entry = map.get(countryName) ?? {
      totalAmount: 0,
      count: 0,
    };

    entry.totalAmount += Math.max(t.baseAmount ?? 0, 0);
    entry.count += 1;

    map.set(countryName, entry);
  });

  return Array.from(map.entries()).map(([country, values]) => ({
    country,
    totalAmount: values.totalAmount,
    count: values.count,
  }));
}, [transactions, countryNameByCode]);

  return {
    totalTransactionsAmount,
    totalTransactionsCount,
    transactionTrend,
    transactionPercentageChange,
    transactionsByType,
    weeklyActivity,
    monthlyVolume,
    transactionsCountry,
  };
}
