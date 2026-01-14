import { useMemo, useState, useCallback } from "react";
import { GeographicDistributionChart } from "@/components/ui/charts/geographicdistributionchart";
import { TransactionsByTypeChart } from "@/components/ui/charts/transactionchart";
import { AdaptiveAreaChart } from "@/components/ui/charts/adaptiveareachart";
import { ChartCard } from "@/components/ui/charts/chartcard";
import { mapTransactionToRow } from "@/mappers/transaction/transaction.mapper";
import { mapCaseDtoToTableRow } from "@/mappers/case/case.mapper";
import { TransactionsTable } from "@/components/ui/tables/transactionstable";
import { Pagination } from "@/components/ui/pagination";
import { DollarSign, ArrowUpRight, ArrowDownRight, 
  ShieldAlert, Minus, Users, TrendingUp, TrendingDown, 
  AlertTriangle} from "lucide-react";

import { useApi } from "@/hooks/useApi";
import { useTransactions } from "@/hooks/useTransactions";
import { fetchTransactions } from "@/services/transaction.service";
import type { PagedTransactionsResponseDto } from "@/types/Transactions/transaction";
import type { PagedClientsResponseDto } from "@/types/Clients/client";
import type { PagedCasesResponseDto } from "@/types/Cases/cases";
import { fetchClients } from "@/services/clients.service";
import { useClients } from "@/hooks/useClients";
import { useCases } from "@/hooks/useCases";
import { fetchCases } from "@/services/case.service";
import { formatCurrencyCompact } from "@/lib/utils";
import { fetchCountries } from "@/services/countries.service";
import type { CountriesResponseDto } from "@/types/Countries/countries";

const PAGE_SIZE = 20;

export function Dashboard() {
  const [currentPage, setCurrentPage] = useState(1);

  const fetchTransactionsWithPagination = useCallback(
    () => fetchTransactions({ page: currentPage, pageSize: PAGE_SIZE }),
    [currentPage]
  );

  const { data: countriesData } = useApi<CountriesResponseDto[]>({
  fetcher: fetchCountries,
  });

  const countries = countriesData ?? [];
  const fetchTransactionsForMetrics = useCallback(
    () => fetchTransactions({ page: 1, pageSize: 2000 }), 
    []
  );

  const { data: metricsData } = useApi<PagedTransactionsResponseDto>({
    fetcher: fetchTransactionsForMetrics,
  });

  const allTransactions = metricsData?.items ?? [];

  const { data: transactionsData } = useApi<PagedTransactionsResponseDto>({
    fetcher: fetchTransactionsWithPagination,
    deps: [currentPage],
  });

  const transactions = transactionsData?.items ?? [];
  
  const transactionRows = useMemo(() => {
    return transactions.map(mapTransactionToRow);
  }, [transactions]);

  const { data: casesData} =
    useApi<PagedCasesResponseDto>({
      fetcher: fetchCases,
    });
  const cases = casesData?.items ?? [];
  
  const casesRows = useMemo(() => {
    return cases.map(mapCaseDtoToTableRow);
  }, [cases]);

  const { 
    activeAlertsCount,
    criticalAlertsCount,
    activeAlertsTrend,
    activeAlertsPercentageChange
   } = useCases(casesRows);

  const {
    totalTransactionsAmount,
    totalTransactionsCount,
    transactionTrend,
    transactionPercentageChange,
    transactionsByType,
    weeklyActivity,
    transactionsCountry
  } = useTransactions(allTransactions, cases);

  const { data : clientsData} = 
    useApi<PagedClientsResponseDto>({
      fetcher: fetchClients
    })

  const clients = clientsData?.items ?? []

  const {
    totalUsersCount,
    newClientsCurrentPeriod,
    clientTrend,
    clientPercentageChange,

    highRiskPercentage,
    highRiskCount,
    highRiskTrend,
    highRiskCurrentPeriod,

  } = useClients(clients)


  return (
    <div className="space-y-6 animate-in fade-in duration-500">
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <div className="rounded-xl bg-card p-6 shadow-sm border border-border flex flex-col justify-between hover:border-primary/50 transition-colors">
          <div className="flex items-start justify-between">
            <div>
              <h3 className="text-sm font-medium text-muted-foreground">
                Total Transactions Volume
              </h3>

              <p className="mt-2 text-3xl font-bold text-foreground">
                ${formatCurrencyCompact(totalTransactionsAmount)}
              </p>

              <p className="mt-1 text-xs text-muted-foreground">
                {totalTransactionsCount} transactions
              </p>
            </div>

            <div className="bg-primary/10 p-3 rounded-lg">
              <DollarSign className="h-5 w-5 text-primary" />
            </div>
          </div>

          <div className="mt-4 text-xs flex items-center gap-1">
            {transactionTrend === "up" && (
              <ArrowUpRight className="h-4 w-4 text-emerald-600" />
            )}
            {transactionTrend === "down" && (
              <ArrowDownRight className="h-4 w-4 text-destructive" />
            )}
            {transactionTrend === "neutral" && (
              <Minus className="h-4 w-4 text-muted-foreground" />
            )}

            {transactionPercentageChange !== null && (
              <span
                className={`text-xs font-medium ${
                  transactionTrend === "up"
                    ? "text-emerald-600"
                    : transactionTrend === "down"
                    ? "text-destructive"
                    : "text-muted-foreground"
                }`}
              >
                {transactionPercentageChange > 0 ? "+" : ""}
                {transactionPercentageChange.toFixed(1)}%
              </span>
            )}

            <span className="text-muted-foreground ml-1">vs last period</span>
          </div>
        </div>

        <div className="rounded-xl bg-card p-6 shadow-sm border border-border flex flex-col justify-between hover:border-destructive/50 transition-colors">
          <div className="flex items-start justify-between mb-3">
            <div>
              <h3 className="text-sm font-medium text-muted-foreground">Active Alerts</h3>
              <p className="mt-2 text-3xl font-bold text-foreground">{activeAlertsCount}</p>
              <p className="mt-1 text-xs text-muted-foreground">{criticalAlertsCount} critical alerts</p>
            </div>
            <div className="rounded-lg bg-destructive/10 p-3">
              <AlertTriangle className="h-5 w-5 text-destructive" />
            </div>
          </div>

          <div className="flex items-center gap-1 text-xs">
            {activeAlertsTrend === "up" && (
              <TrendingUp className="h-4 w-4 text-destructive" />
            )}
            {activeAlertsTrend === "down" && (
              <TrendingDown className="h-4 w-4 text-emerald-600" />
            )}
            {activeAlertsTrend === "neutral" && (
              <Minus className="h-4 w-4 text-muted-foreground" />
            )}

            {activeAlertsPercentageChange !== null && (
              <span
                className={`font-medium ${
                  activeAlertsTrend === "up"
                    ? "text-destructive"
                    : activeAlertsTrend === "down"
                    ? "text-emerald-600"
                    : "text-muted-foreground"
                }`}
              >
                {activeAlertsPercentageChange > 0 ? "+" : ""}
                {activeAlertsPercentageChange.toFixed(1)}%
              </span>
            )}

            <span className="text-muted-foreground ml-1">vs last period</span>
          </div>
        </div>

        <div className="rounded-xl bg-card p-6 shadow-sm border border-border flex flex-col justify-between">
          <div className="flex items-start justify-between mb-3">
            <div>
              <h3 className="text-sm font-medium text-muted-foreground">
                High Risk Clients
              </h3>
              <p className="mt-2 text-3xl font-bold text-foreground">
                {highRiskCount}
              </p>
              <p className="text-xs text-muted-foreground">
                {highRiskPercentage.toFixed(1)}% of total
              </p>
            </div>

            <div className="rounded-lg bg-destructive/10 p-3">
              <ShieldAlert className="h-5 w-5 text-destructive" />
            </div>
          </div>

          <div className="flex items-center gap-1 text-xs">
            {highRiskTrend === "up" && (
              <TrendingUp className="h-4 w-4 text-destructive" />
            )}
            {highRiskTrend === "down" && (
              <TrendingDown className="h-4 w-4 text-emerald-600" />
            )}
            {highRiskTrend === "neutral" && (
              <Minus className="h-4 w-4 text-muted-foreground" />
            )}

            <span className={
              highRiskTrend === "up"
                ? "text-destructive"
                : highRiskTrend === "down"
                ? "text-emerald-600"
                : "text-muted-foreground"
              }
            >
              {highRiskTrend === "up" && "+"}
              {highRiskTrend === "down" && "-"}
              {highRiskCurrentPeriod}</span>
            <span className="text-muted-foreground ml-1"> vs last period</span>
          </div>
        </div>

        <div className="rounded-xl bg-card p-6 shadow-sm border border-border flex flex-col justify-between hover:border-primary/50 transition-colors">
          <div className="flex items-start justify-between mb-3">
            <div>
              <h3 className="text-sm font-medium text-muted-foreground">Total Clients</h3>
              <p className="mt-2 text-3xl font-bold text-foreground">{totalUsersCount}</p>
              <p className="text-xs text-muted-foreground">{newClientsCurrentPeriod} new this week</p>
            </div>

            <div className="bg-secondary p-3 rounded-lg">
              <Users className="w-5 h-5 text-secondary-foreground" />
            </div>
          </div>


          <div className="mt-4 flex items-center gap-1 text-xs">
            {clientTrend === "up" && (
              <TrendingUp className="h-4 w-4 text-emerald-600" />
            )}
            {clientTrend === "down" && (
              <TrendingDown className="h-4 w-4 text-destructive" />
            )}
            {clientTrend === "neutral" && (
              <Minus className="h-4 w-4 text-muted-foreground" />
            )}

            {clientPercentageChange !== null && (
              <span
                className={`text-xs font-medium ${
                  clientTrend === "up"
                    ? "text-emerald-600"
                    : clientTrend === "down"
                    ? "text-destructive"
                    : "text-muted-foreground"
                }`}
              >
                {clientPercentageChange > 0 ? "+" : ""}
                {clientPercentageChange.toFixed(1)}%
              </span>
            )}
            <span className="text-muted-foreground ml-1">vs last period</span>
          </div>
        </div>

      </div>
      <div className="mt-5 grid grid-cols-1 gap-6 lg:grid-cols-2">
        <ChartCard title="Geographic Distribution">
          {countries.length > 0 && (
            <GeographicDistributionChart
              data={transactionsCountry}
            />
          )}

        </ChartCard>

        <ChartCard title="Transactions Type Distribution">
          <TransactionsByTypeChart
            data={transactionsByType} />
        </ChartCard>
      </div>

      <div className="mt-6">
        <ChartCard
          title="Weekly Alerts x Transactions"
        >
          <AdaptiveAreaChart
            data={weeklyActivity}
            xKey="day"
            areas={[
              { key: "alerts", label: "Alerts", color: "#dc2626" },
              { key: "transactions", label: "Transactions", color: "#2563eb" },
            ]}
          />
        </ChartCard>
      </div>

      <div className="mt-5">
        <ChartCard title="Recent Transactions">
          <TransactionsTable transactions={transactionRows} />
          {transactionsData && (
            <Pagination
              currentPage={currentPage}
              totalPages={transactionsData.totalPages}
              totalItems={transactionsData.totalCount}
              pageSize={PAGE_SIZE}
              onPageChange={setCurrentPage}
            />
          )}
        </ChartCard>
      </div>
    </div>
  );
}

    