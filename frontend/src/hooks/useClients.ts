// hooks/useClients.ts
import { useMemo } from "react";
import type { ClientResponseDto } from "@/types/Clients/client";

type Trend = "up" | "down" | "neutral";

interface UseClientsResult {
  totalUsersCount: number;

  newClientsCurrentPeriod: number;
  newClientsPreviousPeriod: number;
  clientTrend: Trend;
  clientPercentageChange: number | null;

  highRiskClients: ClientResponseDto[];
  highRiskCount: number;
  highRiskPercentage: number;
  highRiskCurrentPeriod: number;
  highRiskPreviousPeriod: number;
  highRiskTrend: Trend;
}

export function useClients(
  clients: ClientResponseDto[]
): UseClientsResult {
  const now = new Date();

  const totalUsersCount = clients.length;

  const newClientsCurrentPeriod = useMemo(() => {
    const from = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
    return clients.filter(
      c => new Date(c.createdAtUtc) >= from
    ).length;
  }, [clients]);

  const newClientsPreviousPeriod = useMemo(() => {
    const from = new Date(now.getTime() - 14 * 24 * 60 * 60 * 1000);
    const to = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);

    return clients.filter(c => {
      const d = new Date(c.createdAtUtc);
      return d >= from && d < to;
    }).length;
  }, [clients]);

  const clientTrend: Trend =
    newClientsCurrentPeriod > newClientsPreviousPeriod
      ? "up"
      : newClientsCurrentPeriod < newClientsPreviousPeriod
      ? "down"
      : "neutral";

  const clientPercentageChange =
    newClientsPreviousPeriod === 0
      ? null
      : ((newClientsCurrentPeriod - newClientsPreviousPeriod) /
          newClientsPreviousPeriod) *
        100;

  const highRiskClients = useMemo(() => {
    return clients.filter(c => c.riskLevel === 2);
  }, [clients]);

  const highRiskCount = highRiskClients.length;

  const highRiskPercentage =
    totalUsersCount === 0 ? 0 : (highRiskCount / totalUsersCount) * 100;

    const highRiskCurrentPeriod = useMemo(() => {
    const from = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
    return clients.filter(
      c =>
        c.riskLevel === 2 &&
        new Date(c.createdAtUtc) >= from
    ).length;
  }, [clients]);

  const highRiskPreviousPeriod = useMemo(() => {
    const from = new Date(now.getTime() - 14 * 24 * 60 * 60 * 1000);
    const to = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);

    return clients.filter(c => {
      const d = new Date(c.createdAtUtc);
      return (
        c.riskLevel === 2 &&
        d >= from &&
        d < to
      );
    }).length;
  }, [clients]);

  const highRiskTrend: Trend =
    highRiskCurrentPeriod > highRiskPreviousPeriod
      ? "up"
      : highRiskCurrentPeriod < highRiskPreviousPeriod
      ? "down"
      : "neutral";

  return {
    totalUsersCount,

    newClientsCurrentPeriod,
    newClientsPreviousPeriod,
    clientTrend,
    clientPercentageChange,

    highRiskClients,
    highRiskCount,
    highRiskPercentage,
    highRiskCurrentPeriod,
    highRiskPreviousPeriod,
    highRiskTrend,
  };
}
