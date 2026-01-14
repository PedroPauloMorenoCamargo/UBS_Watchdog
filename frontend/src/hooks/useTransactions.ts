// import { useState, useEffect } from "react";
// import { api } from "@/lib/api";
// import type { CreateTransactionDto } from "@/types/Transactions/transaction";

// export function useTransactions() {
//   const [transactions, setTransactions] = useState<CreateTransactionDto[]>([]);
//   const [loading, setLoading] = useState(false);

//   const fetchTransactions = async () => {
//     setLoading(true);
//     try {
//       const res = await api.get<CreateTransactionDto[]>("/api/transactions");
//       setTransactions(res.data);
//     } catch (err) {
//       console.error("Failed to fetch transactions", err);
//     } finally {
//       setLoading(false);
//     }
//   };

//   useEffect(() => {
//     fetchTransactions();
//   }, []);

//   return {
//     transactions,
//     loading,
//     refetch: fetchTransactions,
//   };
// }

import { useMemo } from "react";
import type { TransactionResponseDto } from "@/types/Transactions/transaction";
import { mapTransactionType } from "@/mappers/transaction/transactionType.mapper";
import { mapCountryCode } from "@/constants/countries";

import { TRANSACTION_TYPES } from "@/constants/transactionTypes";
import { MONTHS } from "@/constants/months";
import { COUNTRY_MAP } from "@/constants/countries";
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

export function useTransactions(transactions: TransactionResponseDto[]): UseTransactionsResult {
  const now = new Date();

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
  const map = new Map<string, WeeklyActivity>();


  const start = new Date();
  start.setUTCHours(0, 0, 0, 0);
  start.setUTCDate(start.getUTCDate() - 6);

  const end = new Date(); 

  
  for (let i = 0; i < 7; i++) {
    const d = new Date(start);
    d.setUTCDate(start.getUTCDate() + i);

    const label = d.toLocaleDateString("en-US", {
      weekday: "short",
    });

    map.set(label, {
      day: label,
      transactions: 0,
      alerts: 0, 
    });
  }

  
  transactions.forEach((t) => {
    const txDate = new Date(t.occurredAtUtc);

    if (txDate >= start && txDate <= end) {
      const label = txDate.toLocaleDateString("en-US", {
        weekday: "short",
      });

      const entry = map.get(label);
      if (entry) {
        entry.transactions += 1;
      }
    }
  });

  return Array.from(map.values());
}, [transactions]);

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
  const map = new Map<string, { totalAmount: number; count: number }>();


  Object.values(COUNTRY_MAP).forEach(code => {
    map.set(code, { totalAmount: 0, count: 0 });
  });
  map.set("Others", { totalAmount: 0, count: 0 });

  transactions.forEach(t => {
    const country = mapCountryCode(t.cpCountryCode);
    const entry = map.get(country)!;
    entry.totalAmount += Math.max(t.baseAmount ?? 0, 0);
    entry.count += 1;
    map.set(country, entry);
  });

  return Array.from(map.entries()).map(([country, values]) => ({
    country,
    totalAmount: values.totalAmount,
    count: values.count,
  }));
}, [transactions]);


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
