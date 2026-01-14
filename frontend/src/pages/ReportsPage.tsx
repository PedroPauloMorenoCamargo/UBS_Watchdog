import { useState, useEffect, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { Download, FileText } from "lucide-react";
import { ChartCard } from "@/components/ui/charts/chartcard";
import { ReportsTable } from "@/components/ui/tables/reportstable";
import { reportsMock } from "@/mocks/mocks";
import { useApi } from "@/hooks/useApi";
import { useTransactions } from "@/hooks/useTransactions";
import { useClients } from "@/hooks/useClients";
import { fetchTransactions } from "@/services/transaction.service";
import { fetchClients } from "@/services/clients.service";
import { fetchCases } from "@/services/case.service";
import { exportSystemReportCsv } from "@/services/reports.service";
import { mapPagedCasesDtoToTableRows } from "@/mappers/case/case.mapper";
import { ClientSearchSelect } from "@/components/ui/clientsearchselect";
import { UsersByRiskLevelChart } from "@/components/ui/charts/userrisklevelchart";
import { AdaptiveLineChart } from "@/components/ui/charts/adaptivelinechart";
import jsPDF from "jspdf";
import { domToPng } from "modern-screenshot";
import { useCases } from "@/hooks/useCases";
import { fetchAllClientsReports } from "@/services/reports.service";

import type { PagedClientsResponseDto } from "@/types/Clients/client";
import type { PagedTransactionsResponseDto } from "@/types/Transactions/transaction";
import type { PagedCasesResponseDto } from "@/types/Cases/cases";
import { AlertSeverityBarChart } from "@/components/ui/charts/alertsbyseveritychart";
import { StatCard } from "@/components/ui/statcard";
import { useReports } from "@/hooks/useReport";

export function ReportsPage() {
  const navigate = useNavigate();
  const [selectedClientId, setSelectedClientId] = useState<string>("");
  const [isExporting, setIsExporting] = useState(false);

  const { data: casesData } = useApi<PagedCasesResponseDto>({ fetcher: fetchCases });
  const { data: clientsData } = useApi<PagedClientsResponseDto>({ fetcher: fetchClients });
  const { data: transactionsData } = useApi<PagedTransactionsResponseDto>({ fetcher: fetchTransactions });

  const cases = casesData?.items ?? []
  const mappedCases = mapPagedCasesDtoToTableRows({ items: cases });
  const clients = clientsData?.items ?? [];
  const transactions = transactionsData?.items ?? [];
  const { reports: allReports, isLoading } = useReports(clients);
  const { decisionsCount, weeklyAlertsBySeverity } = useCases(mappedCases);
  const { usersByRiskLevel } = useClients(clients);
  const { monthlyVolume } = useTransactions(transactions, cases);

  const clientOptions = useMemo(() => clients.map(c => ({ value: c.id, label: c.name })), [clients]);

  useEffect(() => {
    if (selectedClientId) {
      navigate(`/reports/client/${selectedClientId}`);
      setSelectedClientId("");
    }
  }, [selectedClientId, navigate]);

  const handleExportCsv = async () => {
    setIsExporting(true);
    try {
      const blob = await exportSystemReportCsv();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `system-report-${new Date().toISOString().split("T")[0]}.csv`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (err) {
      console.error("Error exporting CSV:", err);
      alert("Failed to export CSV. Please try again.");
    } finally {
      setIsExporting(false);
    }
  };

  const handleExportPdf = async () => {
    setIsExporting(true);
    const element = document.getElementById("reports-content");
    if (!element) {
      alert("Content not found");
      setIsExporting(false);
      return;
    }

    try {
      const imgData = await domToPng(element, { quality: 0.95, backgroundColor: "#ffffff", scale: 2 });
      const pdf = new jsPDF({ orientation: "portrait", unit: "mm", format: "a4" });
      const pdfWidth = pdf.internal.pageSize.getWidth();
      const imgProps = pdf.getImageProperties(imgData);
      const imgWidth = pdfWidth - 20;
      const imgHeight = (imgProps.height * imgWidth) / imgProps.width;
      pdf.addImage(imgData, "PNG", 10, 10, imgWidth, imgHeight);
      pdf.save(`compliance-reports-${new Date().toISOString().split("T")[0]}.pdf`);
    } catch (err) {
      console.error("Error generating PDF:", err);
      alert("Failed to generate PDF. Please try again.");
    } finally {
      setIsExporting(false);
    }
  };          
  console.log(weeklyAlertsBySeverity)
  console.log(cases)


  return (
    <div className="relative bg-cover bg-center">
      <div className="relative z-10 p-3">

        <div className="mb-6 flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-slate-800">Compliance Reports</h1>
            <p className="text-sm text-slate-500">System-wide compliance and transaction analysis</p>
          </div>
          <div className="flex gap-2">
            <button onClick={handleExportCsv} disabled={isExporting} className="flex items-center gap-2 px-4 py-2 border border-slate-300 rounded-lg bg-white hover:bg-slate-50 text-sm font-medium disabled:opacity-50 disabled:cursor-not-allowed">
              <Download className="h-4 w-4" /> Export CSV
            </button>
            <button onClick={handleExportPdf} disabled={isExporting} className="flex items-center gap-2 px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 text-sm font-medium disabled:opacity-50 disabled:cursor-not-allowed">
              <FileText className="h-4 w-4" /> Export PDF
            </button>
          </div>
        </div>

        <div className="mb-6 rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="text-sm font-semibold text-red-600">Generate Client-Specific Report</h3>
              <p className="text-xs text-slate-500 mt-0.5">Select a client to view detailed compliance analysis, transaction history, and risk assessment</p>
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

        <div id="reports-content">

          <div className="grid gap-6 [grid-template-columns:repeat(auto-fit,minmax(240px,1fr))]">
                  <StatCard title="Fraudulent" value={decisionsCount.fraudulent} variant="destructive" />
                  <StatCard title="Not Fraudulent" value={decisionsCount.notFraudulent} variant="low" />
                  <StatCard title="Inconclusive" value={decisionsCount.inconclusive} variant="default" />
                </div>

          

          {/* ==== OTHER CHARTS ==== */}
          <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-2">
            <ChartCard title="Transaction Volume by Month">
              <AdaptiveLineChart
                data={monthlyVolume}
                xKey="month"
                showLegend={false}
                lines={[{ key: "volume", label: "Transaction Volume", color: "#fa0101a2" }]}
              />
            </ChartCard>

            <ChartCard title="Users by Risk Level">
              <UsersByRiskLevelChart data={usersByRiskLevel} />
            </ChartCard>
          </div>
          {/* ==== ALERTS BY SEVERITY CHART ==== */}
          <div className="mt-6">
            <ChartCard title="Alert Trends (Last 7 Days)">
              <AlertSeverityBarChart
                data={weeklyAlertsBySeverity}
                
              />
            </ChartCard>
          </div>
          {/* ==== GENERATED REPORTS TABLE ==== */}
          <div className="mt-5">
            <ChartCard title="Generated Reports">
              {isLoading ? (
                <p className="text-center py-6">Loading reports...</p>
              ) : (
                <ReportsTable reports={allReports ?? []} />
              )}
            </ChartCard>
          </div>

        </div>
      </div>
    </div>
  );
}
