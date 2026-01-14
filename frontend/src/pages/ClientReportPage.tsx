import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Download, FileText } from "lucide-react";
import { ChartCard } from "@/components/ui/charts/chartcard";
import { TransactionVolumeChart } from "@/components/ui/charts/transactionvolumechart";
import { AlertsByTypePieChart } from "@/components/ui/charts/alertsbytypepiechart";
import { fetchClientReport, fetchClientDetail } from "@/services/reports.service";
import { exportClientReportCsv } from "@/services/clientReportExport.service";
import jsPDF from "jspdf";
import { domToPng } from "modern-screenshot";
import type { ClientReportDto, ClientDetailDto } from "@/types/Reports/report";

const KYC_STATUS_LABELS: Record<number, string> = {
  0: "Pending",
  1: "Verified",
  2: "Expired",
  3: "Rejected",
};

const RISK_LEVEL_LABELS: Record<number, string> = {
  0: "Low",
  1: "Medium",
  2: "High",
};

const RISK_LEVEL_COLORS: Record<number, string> = {
  0: "bg-green-500 text-white",
  1: "bg-yellow-500 text-white",
  2: "bg-red-500 text-white",
};

export function ClientReportPage() {
  const { clientId } = useParams<{ clientId: string }>();
  const navigate = useNavigate();
  const [clientReport, setClientReport] = useState<ClientReportDto | null>(null);
  const [clientDetail, setClientDetail] = useState<ClientDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!clientId) {
      navigate("/reports");
      return;
    }

    const loadClientData = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const [report, detail] = await Promise.all([
          fetchClientReport(clientId),
          fetchClientDetail(clientId),
        ]);
        setClientReport(report);
        setClientDetail(detail);
      } catch (err) {
        console.error("Error loading client data:", err);
        setError("Failed to load client data. Please try again.");
      } finally {
        setIsLoading(false);
      }
    };

    loadClientData();
  }, [clientId, navigate]);

  // Transform transaction trend data for bar chart (12 months)
  const transactionVolumeData = (() => {
    const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    // Create a map with all 12 months initialized to 0
    const monthlyData: Record<string, number> = {};
    months.forEach((m) => (monthlyData[m] = 0));

    // Fill in actual data from API
    clientReport?.transactionTrend?.forEach((item) => {
      const monthName = new Date(item.date).toLocaleDateString("en-US", {
        month: "short",
      });
      if (monthlyData[monthName] !== undefined) {
        monthlyData[monthName] += item.volumeUSD;
      }
    });

    // Convert to array format
    return months.map((month) => ({
      month,
      volume: monthlyData[month],
    }));
  })();

  // Transform alerts by severity for pie chart
  const alertsByTypeData = clientReport
    ? [
      { name: "Low", value: clientReport.caseMetrics.lowSeverityCases },
      { name: "Medium", value: clientReport.caseMetrics.mediumSeverityCases },
      { name: "High", value: clientReport.caseMetrics.highSeverityCases },
      { name: "Critical", value: clientReport.caseMetrics.criticalSeverityCases },
    ]
    : [];

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="h-10 w-10 animate-spin rounded-full border-4 border-slate-200 border-t-red-500"></div>
        <span className="ml-3 text-slate-600">Loading client report...</span>
      </div>
    );
  }

  if (error || !clientDetail || !clientReport) {
    return (
      <div className="p-6">
        <button
          onClick={() => navigate("/reports")}
          className="flex items-center gap-2 text-slate-600 hover:text-slate-800 mb-4"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Summary
        </button>
        <div className="rounded-xl bg-white p-8 shadow-lg text-center">
          <p className="text-red-500">{error || "Client not found"}</p>
        </div>
      </div>
    );
  }

  // Export CSV handler
  const handleExportCsv = async () => {
    if (!clientId) return;
    try {
      const blob = await exportClientReportCsv(clientId);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `client-report-${clientId}.csv`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (err) {
      alert("Failed to export CSV. Please try again.");
    }
  };

  // Export PDF handler
  const handleExportPdf = async () => {
    const element = document.getElementById("client-report-content");
    if (!element) {
      alert("Content not found");
      return;
    }
    try {
      const imgData = await domToPng(element, {
        quality: 0.95,
        backgroundColor: "#ffffff",
        scale: 2,
      });
      const pdf = new jsPDF({
        orientation: "portrait",
        unit: "mm",
        format: "a4",
      });
      const pdfWidth = pdf.internal.pageSize.getWidth();
      const imgProps = pdf.getImageProperties(imgData);
      const imgWidth = pdfWidth - 20;
      const imgHeight = (imgProps.height * imgWidth) / imgProps.width;
      pdf.addImage(imgData, "PNG", 10, 10, imgWidth, imgHeight);
      pdf.save(`client-report-${clientId}.pdf`);
    } catch (err) {
      alert("Failed to generate PDF. Please try again.");
    }
  };

  return (
    <div className="p-6 bg-slate-50 min-h-screen">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <button
            onClick={() => navigate("/reports")}
            className="flex items-center gap-2 text-slate-600 hover:text-slate-800 mb-2"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Summary
          </button>
          <h1 className="text-2xl font-bold text-slate-800">
            Client Report: {clientDetail.name}
          </h1>
          <p className="text-sm text-slate-500">
            Comprehensive compliance and transaction analysis
          </p>
        </div>
        <div className="flex gap-2">
          <button onClick={handleExportCsv} className="flex items-center gap-2 px-4 py-2 border border-slate-300 rounded-lg bg-white hover:bg-slate-50 text-sm font-medium">
            <Download className="h-4 w-4" />
            Export CSV
          </button>
          <button onClick={handleExportPdf} className="flex items-center gap-2 px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 text-sm font-medium">
            <FileText className="h-4 w-4" />
            Export PDF
          </button>
        </div>
      </div>

      {/* Conteúdo para exportação */}
      <div id="client-report-content">
        {/* Client Information Section */}
        <div className="rounded-xl bg-white p-6 shadow-sm mb-6">
          <h2 className="text-lg font-semibold text-slate-700 mb-4">Client Information</h2>
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-6">
            <div>
              <p className="text-xs text-slate-500 uppercase tracking-wide">Client ID</p>
              <p className="text-sm font-medium mt-1">{clientDetail.id}</p>
            </div>
            <div>
              <p className="text-xs text-slate-500 uppercase tracking-wide">Country</p>
              <p className="text-sm font-medium mt-1">{clientDetail.countryCode}</p>
            </div>
            <div>
              <p className="text-xs text-slate-500 uppercase tracking-wide">Account Type</p>
              <p className="text-sm font-medium mt-1">
                {clientDetail.legalType === 0 ? "Individual" : "Corporate"}
              </p>
            </div>
            <div>
              <p className="text-xs text-slate-500 uppercase tracking-wide">Registration Date</p>
              <p className="text-sm font-medium mt-1">
                {new Date(clientDetail.createdAtUtc).toLocaleDateString()}
              </p>
            </div>
            <div>
              <p className="text-xs text-slate-500 uppercase tracking-wide">Total Transactions</p>
              <p className="text-sm font-medium mt-1">
                {clientReport.transactionMetrics.totalTransactions}
              </p>
            </div>
            <div>
              <p className="text-xs text-slate-500 uppercase tracking-wide">Total Volume</p>
              <p className="text-sm font-medium mt-1">
                ${clientReport.transactionMetrics.totalVolumeUSD.toLocaleString()}
              </p>
            </div>
            <div>
              <p className="text-xs text-slate-500 uppercase tracking-wide">Active Alerts</p>
              <p className="text-sm font-medium mt-1">{clientReport.caseMetrics.totalCases}</p>
            </div>
            <div>
              <p className="text-xs text-slate-500 uppercase tracking-wide">Risk Level</p>
              <span
                className={`inline-block mt-1 px-2 py-0.5 text-xs font-medium rounded ${RISK_LEVEL_COLORS[clientDetail.riskLevel] ?? "bg-slate-100"
                  }`}
              >
                {RISK_LEVEL_LABELS[clientDetail.riskLevel]?.toUpperCase() ?? "UNKNOWN"}
              </span>
            </div>
          </div>
        </div>

        {/* KYC Status */}
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-4 mb-6">
          <div className="rounded-xl bg-white p-4 shadow-sm">
            <p className="text-xs text-slate-500 uppercase tracking-wide">KYC Status</p>
            <p className="text-sm font-medium mt-2">
              {KYC_STATUS_LABELS[clientDetail.kycStatus]?.toLowerCase() ?? "unknown"}
            </p>
          </div>
        </div>

        {/* Charts Section */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
          <ChartCard title="Monthly Transaction Volume (12 Months)">
            {transactionVolumeData.some((d) => d.volume > 0) ? (
              <TransactionVolumeChart data={transactionVolumeData} height={280} />
            ) : (
              <div className="flex items-center justify-center h-64 text-slate-500">
                No transaction data available
              </div>
            )}
          </ChartCard>

          <ChartCard title="Alerts by Severity">
            <AlertsByTypePieChart data={alertsByTypeData} height={280} />
          </ChartCard>
        </div>

        {/* Transaction Metrics Cards */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="rounded-xl bg-white p-4 shadow-sm">
            <p className="text-xs text-slate-500 uppercase tracking-wide">Deposits</p>
            <p className="text-xl font-bold mt-2">
              {clientReport.transactionMetrics.depositCount}
            </p>
          </div>
          <div className="rounded-xl bg-white p-4 shadow-sm">
            <p className="text-xs text-slate-500 uppercase tracking-wide">Withdrawals</p>
            <p className="text-xl font-bold mt-2">
              {clientReport.transactionMetrics.withdrawalCount}
            </p>
          </div>
          <div className="rounded-xl bg-white p-4 shadow-sm">
            <p className="text-xs text-slate-500 uppercase tracking-wide">Transfers</p>
            <p className="text-xl font-bold mt-2">
              {clientReport.transactionMetrics.transferCount}
            </p>
          </div>
          <div className="rounded-xl bg-white p-4 shadow-sm">
            <p className="text-xs text-slate-500 uppercase tracking-wide">Avg Transaction</p>
            <p className="text-xl font-bold mt-2">
              ${clientReport.transactionMetrics.averageTransactionUSD.toLocaleString()}
            </p>
          </div>
        </div> {/* Fim do id="client-report-content" */}
      </div>
    </div>
  );
}
