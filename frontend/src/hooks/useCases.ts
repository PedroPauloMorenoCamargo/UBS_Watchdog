import { useMemo } from "react";
import type { CaseTableRow } from "@/models/case";

type Trend = "up" | "down" | "neutral";

function getTrend(current: number, previous: number): Trend {
  if (current > previous) return "up";
  if (current < previous) return "down";
  return "neutral";
}

function getPercentageChange(current: number, previous: number): number | null {
  if (previous === 0) return null;
  return ((current - previous) / previous) * 100;
}

export function useCases(cases: CaseTableRow[]) {
  const now = new Date();
  const currentPeriodStart = new Date();
  currentPeriodStart.setDate(now.getDate() - 7);

  const previousPeriodStart = new Date();
  previousPeriodStart.setDate(now.getDate() - 14);

 
  const activeCases = useMemo(() => {
    return cases.filter(
      (c) => c.status === "New" || c.status === "Under Review"
    );
  }, [cases]);

  const activeAlertsCount = activeCases.length;


  const criticalAlertsCount = useMemo(() => {
    return activeCases.filter((c) => c.severity.toLowerCase() === "critical").length;
  }, [activeCases]);

  const highAlertsCount = useMemo(() => {
    return activeCases.filter((c) => c.severity.toLowerCase() === "high").length;
  }, [activeCases]);

  const mediumAlertsCount = useMemo(() => {
    return activeCases.filter((c) => c.severity.toLowerCase() === "medium").length;
  }, [activeCases]);

  const lowAlertsCount = useMemo(() => {
    return activeCases.filter((c) => c.severity.toLowerCase() === "low").length;
  }, [activeCases]);


  const activeCurrentPeriod = useMemo(() => {
    return activeCases.filter((c) => {
      const opened = new Date(c.openedAtUtc);
      return opened >= currentPeriodStart;
    }).length;
  }, [activeCases, currentPeriodStart]);


  const activePreviousPeriod = useMemo(() => {
    return activeCases.filter((c) => {
      const opened = new Date(c.openedAtUtc);
      return (
        opened >= previousPeriodStart &&
        opened < currentPeriodStart
      );
    }).length;
  }, [activeCases, previousPeriodStart, currentPeriodStart]);

  const activeAlertsTrend = getTrend(
    activeCurrentPeriod,
    activePreviousPeriod
  );

  const activeAlertsPercentageChange = getPercentageChange(
    activeCurrentPeriod,
    activePreviousPeriod
  );

  
  const resolvedCurrentPeriod = useMemo(() => {
    return cases.filter((c) => {
      if (!c.resolvedAt) return false;
      const resolved = new Date(c.resolvedAt);
      return resolved >= currentPeriodStart;
    }).length;
  }, [cases, currentPeriodStart]);

  return {
    activeAlertsCount,
    criticalAlertsCount,
    highAlertsCount,
    mediumAlertsCount,
    lowAlertsCount,

    activeCurrentPeriod,
    activePreviousPeriod,
    activeAlertsTrend,
    activeAlertsPercentageChange,

    resolvedCurrentPeriod,
  };
}

