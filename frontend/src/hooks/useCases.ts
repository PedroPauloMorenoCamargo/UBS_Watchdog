import { useMemo } from "react";
import type { CaseTableRow } from "@/models/case";
import { CaseSeverity } from "@/types/Cases/cases";
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
    return activeCases.filter((c) => c.severity === "critical").length;
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

  const decisionsCount = useMemo(() => {
    const fraudulent = cases.filter(c => c.decision === 0).length;
    const notFraudulent = cases.filter(c => c.decision === 1).length;
    const inconclusive = cases.filter(c => c.decision === 2).length;
    return { fraudulent, notFraudulent, inconclusive };
  }, [cases]);

  // =====================
  // ALERTS BY SEVERITY (ULTIMOS 7 DIAS)
  // =====================
 const weeklyAlertsBySeverity = useMemo(() => {
  const result = [];
  const now = new Date();

  // domingo da semana atual
  const startDate = new Date(now);
  startDate.setUTCDate(now.getUTCDate() - now.getUTCDay()); // Sunday

  for (let i = 0; i < 7; i++) {
    const date = new Date(startDate);
    date.setUTCDate(startDate.getUTCDate() + i);

    const dayCases = cases.filter(c => {
      const opened = new Date(c.openedAtUtc);
      return (
        opened.getUTCDate() === date.getUTCDate() &&
        opened.getUTCMonth() === date.getUTCMonth() &&
        opened.getUTCFullYear() === date.getUTCFullYear()
      );
    });

    result.push({
      day: date.toLocaleDateString("en-US", { weekday: "short" }),
      low: dayCases.filter(c => c.severity === "low").length,
      medium: dayCases.filter(c => c.severity === "medium").length,
      high: dayCases.filter(c => c.severity === "high").length,
      critical: dayCases.filter(c => c.severity === "critical").length,
    });
  }

  return result;
}, [cases]);





  return {
    activeAlertsCount,
    criticalAlertsCount,

    activeCurrentPeriod,
    activePreviousPeriod,
    activeAlertsTrend,
    activeAlertsPercentageChange,

    resolvedCurrentPeriod,

    decisionsCount,
    weeklyAlertsBySeverity
  };
}

