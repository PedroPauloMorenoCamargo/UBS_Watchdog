import { useState, useEffect, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { AdaptiveLineChart } from "@/components/ui/charts/adaptivelinechart";
import { ChartCard } from "@/components/ui/charts/chartcard";
import { ReportsTable } from "@/components/ui/tables/reportstable";
import { reportsMock } from "@/mocks/mocks";
import { useApi } from "@/hooks/useApi";
import { useTransactions } from "@/hooks/useTransactions";
import { useClients } from "@/hooks/useClients";
import { fetchTransactions } from "@/services/transaction.service";
import { fetchClients } from "@/services/clients.service";
import type { PagedClientsResponseDto } from "@/types/Clients/client";
import type { PagedTransactionsResponseDto } from "@/types/Transactions/transaction";
import { UsersByRiskLevelChart } from "@/components/ui/charts/userrisklevelchart";
import { ClientSearchSelect } from "@/components/ui/clientsearchselect";

export function ReportsPage() {
  const navigate = useNavigate();
  const [selectedClientId, setSelectedClientId] = useState<string>("");

  const { data: clientsData } = useApi<PagedClientsResponseDto>({
    fetcher: fetchClients,
  });

  const clients = clientsData?.items ?? [];

  const { usersByRiskLevel } = useClients(clients);

  const { data: transactionsData } = useApi<PagedTransactionsResponseDto>({
    fetcher: fetchTransactions,
  });

  const transactions = transactionsData?.items ?? [];

  const { monthlyVolume } = useTransactions(transactions);

  // Transform clients to combobox options
  const clientOptions = useMemo(
    () => clients.map((c) => ({ value: c.id, label: c.name })),
    [clients]
  );

  // Navigate to client report page when a client is selected
  useEffect(() => {
    if (selectedClientId) {
      navigate(`/reports/client/${selectedClientId}`);
      setSelectedClientId("");
    }
  }, [selectedClientId, navigate]);

  return (
    <div className="relative bg-cover bg-center">
      <div className="relative z-10 p-3">
        {/* Generate Client-Specific Report Card */}
        <div className="mb-6 rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="text-sm font-semibold text-red-600">
                Generate Client-Specific Report
              </h3>
              <p className="text-xs text-slate-500 mt-0.5">
                Select a client to view detailed compliance analysis, transaction history, and risk assessment
              </p>
            </div>
            <ClientSearchSelect
              options={clientOptions}
              value={selectedClientId}
              onValueChange={setSelectedClientId}
              placeholder="Select a client"
              searchPlaceholder="Search clients..."
            />
          </div>
        </div>

        {/* Global Stats Section */}
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
{/*           
          <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-2 text-lg font-semibold">Average Resolution Time</h3>
            <p className="text-3xl font-bold text-black-600">4.2 hrs</p> 
             <p className="text-sm mt-2 font-bold text-green-600">-12% vs. last month</p> 
          </div> */}

          <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-2 text-lg font-semibold">Total Alerts Processed</h3>
            <p className="text-3xl font-bold text-black-600">1,248</p> {/*mock*/}
             <p className="text-sm mt-2 font-bold text-gray-500">Last 30 days</p> {/*mock*/}
          </div>
          
          {/* <div className="rounded-xl bg-white/90 p-6 shadow-lg">
            <h3 className="mb-2 text-lg font-semibold">False Positive Rate</h3>
            <p className="text-3xl font-bold text-black-600">18%</p> 
            <p className="text-sm mt-2 font-bold text-red-600">+2% vs. last month</p>
          </div> */}

        </div>

        {/*  PIE CHART CLIENTS BY RISK LEVEL */}

        <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-2">
          <ChartCard title="Transaction Volume by Month">
            <AdaptiveLineChart
              data={monthlyVolume}
              xKey="month"
              showLegend= {false}
              lines={[
                {
                  key: "volume",
                  label: "Transaction Volume",
                  color: "#fa0101a2",
                },
              ]}
            />
          </ChartCard>

          <ChartCard title="Users by Risk Level">
            <UsersByRiskLevelChart data={usersByRiskLevel} />
          </ChartCard>
        </div>
        
        {/* 
          TODO: ALTERAR PARA GRAFICO DE COLUNAS
          Eixo X dias, colunas com os tipos de alertas
          eixo y quantidade de alertas naquele dia

        <div className="mt-6">
          <ChartCard
            title="Alert Trends (Last 7 Days)"
          >
            <AdaptiveLineChart
                data={weeklyAlertsBySeverity}
                xKey="day"
                lines={[
                    { key: "high", label: "High", color: "#dc2626" },
                    { key: "medium", label: "Medium", color: "#f59e0b" },
                    { key: "low", label: "Low", color: "#16a34a" },
                ]}
            />
          </ChartCard>
        </div> */}

        <div className="mt-5">
          <ChartCard title="Generated Reports">
            <ReportsTable reports={reportsMock}/>
          </ChartCard>
        </div>
      </div>
    </div>
  );
}

    