import { useState, useEffect, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { Download, FileText } from "lucide-react";
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
import { fetchCases } from "@/services/case.service";
import type { PagedCasesResponseDto } from "@/types/Cases/cases";
import { ClientSearchSelect } from "@/components/ui/clientsearchselect";
import { exportSystemReportCsv } from "@/services/reports.service";
import jsPDF from "jspdf";
import { domToPng } from "modern-screenshot";

export function ReportsPage() {
  const navigate = useNavigate();
  const [selectedClientId, setSelectedClientId] = useState<string>("");
  const [isExporting, setIsExporting] = useState(false);

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
      // Converte DOM para imagem PNG usando modern-screenshot
      const imgData = await domToPng(element, {
        quality: 0.95,
        backgroundColor: "#ffffff",
        scale: 2,
      });

      // Cria PDF e adiciona a imagem
      const pdf = new jsPDF({
        orientation: "portrait",
        unit: "mm",
        format: "a4",
      });

      const pdfWidth = pdf.internal.pageSize.getWidth();
      const imgProps = pdf.getImageProperties(imgData);
      const imgWidth = pdfWidth - 20; // margem de 10mm de cada lado
      const imgHeight = (imgProps.height * imgWidth) / imgProps.width;

      pdf.addImage(imgData, "PNG", 10, 10, imgWidth, imgHeight);
      pdf.save(`compliance-reports-${new Date().toISOString().split("T")[0]}.pdf`);
      setIsExporting(false);
    } catch (err) {
      console.error("Error generating PDF:", err);
      alert("Failed to generate PDF. Please try again.");
      setIsExporting(false);
    }
  };

   const { data: casesData} =
     useApi<PagedCasesResponseDto>({
       fetcher: fetchCases,
     });
     const cases = casesData?.items ?? [];

  const { data: clientsData } = useApi<PagedClientsResponseDto>({
    fetcher: fetchClients,
  });

  const clients = clientsData?.items ?? [];

  const { usersByRiskLevel } = useClients(clients);

  const { data: transactionsData } = useApi<PagedTransactionsResponseDto>({
    fetcher: fetchTransactions,
  });

  const transactions = transactionsData?.items ?? [];

  const { monthlyVolume } = useTransactions(transactions,cases,[]);

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
        {/* Header with Export Buttons */}
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-slate-800">Compliance Reports</h1>
            <p className="text-sm text-slate-500">System-wide compliance and transaction analysis</p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={handleExportCsv}
              disabled={isExporting}
              className="flex items-center gap-2 px-4 py-2 border border-slate-300 rounded-lg bg-white hover:bg-slate-50 text-sm font-medium disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Download className="h-4 w-4" />
              Export CSV
            </button>
            <button
              onClick={handleExportPdf}
              disabled={isExporting}
              className="flex items-center gap-2 px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 text-sm font-medium disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <FileText className="h-4 w-4" />
              Export PDF
            </button>
          </div>
        </div>

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

        {/* Content to be exported */}
        <div id="reports-content">
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
    </div>
  );
}

    