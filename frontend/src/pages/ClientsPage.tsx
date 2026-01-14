import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useMemo, useState, useRef, useCallback } from "react";
import type { SeverityFilter } from "@/types/alert";
import type { KycFilter } from "@/types/kycstatus";
import { useCountriesList } from "@/hooks/useCountriesList";
import { Search } from "lucide-react";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import { useApi } from "@/hooks/useApi";
import { fetchClients } from "@/services/clients.service";
import { mapClientToTableRow } from "@/mappers/client/client.mapper";
import { ChartCard } from "@/components/ui/charts/chartcard";
import { ClientsTable } from "@/components/ui/tables/clientstable";
import { useImportClientsCsv } from "@/hooks/useImportCsv";
import { CreateClientDialog } from "@/components/ui/dialogs/create-client-dialog";
import { ClientDetailsDialog } from "@/components/ui/dialogs/client-details-dialog";
import { Pagination } from "@/components/ui/pagination";
import { ToastContainer } from "@/components/ui/toast";
import { useToast } from "@/hooks/useToast";

const PAGE_SIZE = 20;

export function ClientsPage() {
  const [search, setSearch] = useState("");
  const [risk, setRisk] = useState<SeverityFilter>("all");
  const [countries, setCountries] = useState<string>("all");
  const [kyc, setKyc] = useState<KycFilter>("all");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [detailsDialogOpen, setDetailsDialogOpen] = useState(false);
  const [selectedClientId, setSelectedClientId] = useState<string | null>(null);

  const { toasts, removeToast, success, error: showError } = useToast();
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const fetchClientsWithPagination = useCallback(
    () => fetchClients({
      page: currentPage,
      pageSize: PAGE_SIZE,
      searchTerm: search,
      riskLevel: risk === "all" ? undefined : risk,
      countryCode: countries === "all" ? undefined : countries,
      kycStatus: kyc === "all" ? undefined : kyc
    }),
    [currentPage, search, risk, countries, kyc]
  );

  const { data, loading, error, refetch } = useApi({
    fetcher: fetchClientsWithPagination,
    deps: [currentPage, search, risk, countries, kyc],
  });

  const {
    submit: importCsv,
    loading: isImporting,
  } = useImportClientsCsv(() => {
    setCurrentPage(1); // Reset to first page after import
    refetch();
  });

  const { countries: countryList } = useCountriesList();

  const clients = useMemo(() => {
    if (!data) return [];
    return data.items.map(mapClientToTableRow);
  }, [data]);

  async function handleImportCSV(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!file.name.toLowerCase().endsWith(".csv")) {
      showError("Apenas arquivos CSV são permitidos!");
      event.target.value = ""; 
      return;
    }

    await importCsv(file);
    success("CSV importado com sucesso!");
    event.target.value = "";
  }

  function handleViewClient(clientId: string) {
    setSelectedClientId(clientId);
    setDetailsDialogOpen(true);
  }
  

  return (
    <div className="relative w-full max-w-full overflow-x-hidden">
      <ToastContainer toasts={toasts} onRemove={removeToast} />
      
      <div className="mt-4 rounded-xl bg-white p-5 shadow">
        <div
          className="grid gap-4 items-end
            grid-cols-1
            sm:grid-cols-2
            lg:grid-cols-[2fr_1.5fr_1.5fr_1.5fr_auto]
          "
        >
          <div>
            <label className="text-xs font-medium text-slate-500">
              Search Client
            </label>
            <div className="relative">
              <Search className="absolute left-0 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
              <Input
                placeholder="Search by client name..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="h-9 rounded-none border-0 border-b border-slate-300 pl-6 pr-0 shadow-none focus-visible:ring-0"
              />
            </div>
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Risk</label>
            <Select
              value={risk}
              onValueChange={(v) => setRisk(v as SeverityFilter)}
            >
              <SelectTrigger className="h-9 w-full">
                <SelectValue placeholder="All" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All</SelectItem>
                <SelectItem value="high">High</SelectItem>
                <SelectItem value="medium">Medium</SelectItem>
                <SelectItem value="low">Low</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">
              Countries
            </label>
            <Select value={countries} onValueChange={(v) => setCountries(v)}>
              <SelectTrigger className="h-9 w-full">
                <SelectValue placeholder="All" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All</SelectItem>
                {countryList.map((c) => (
                  <SelectItem key={c.code} value={c.code}>
                    {c.code}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">
              KYC Status
            </label>
            <Select
              value={kyc}
              onValueChange={(v) => setKyc(v as KycFilter)}
            >
              <SelectTrigger className="h-9 w-full">
                <SelectValue placeholder="All" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All</SelectItem>
                <SelectItem value="expired">Expired</SelectItem>
                <SelectItem value="pending">Pending</SelectItem>
                <SelectItem value="verified">Verified</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex justify-end">
            <Button
              variant="outline"
              className="h-9"
              onClick={() => {
                setSearch("");
                setRisk("all");
                setKyc("all");
                setCountries("all");
              }}
            >
              Clear
            </Button>
          </div>
        </div>
      </div>

      <input
        type="file"
        accept=".csv"
        ref={fileInputRef}
        className="hidden"
        onChange={handleImportCSV}
        disabled={isImporting}
      />

      <div className="mt-4 rounded-xl bg-white p-4 shadow">
        <div className="flex flex-wrap items-center gap-3">
          <Button
            className="cursor-pointer hover:bg-slate-600"
            onClick={() => setDialogOpen(true)}
          >
            Create Client
          </Button>

          <Button
            className="cursor-pointer hover:bg-slate-200"
            variant="outline"
            onClick={() => fileInputRef.current?.click()}
            disabled={isImporting}
          >
            {isImporting ? "Importing..." : "Import CSV"}
          </Button>
        </div>
      </div>

      <div className="mt-5 overflow-x-auto max-w-full">
        <ChartCard title="Clients">
          {error && <p className="text-red-500">{error}</p>}
          
          {loading && !data && <p className="p-4">Loading...</p>}

          {data && (
            <div className={loading ? "opacity-60 transition-opacity pointer-events-none" : ""}>
              <ClientsTable 
                clients={clients} 
                onViewClient={handleViewClient}
              />
              
              <Pagination
                currentPage={currentPage}
                totalPages={data.totalPages}
                totalItems={data.total}
                pageSize={PAGE_SIZE}
                onPageChange={setCurrentPage}
              />
            </div>
          )}
        </ChartCard>
      </div>

      <CreateClientDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        onSuccess={() => {
          refetch();
          success("Cliente criado com sucesso!");
          setDialogOpen(false);
        }}
      />

      {selectedClientId && (
        <ClientDetailsDialog
          open={detailsDialogOpen}
          onOpenChange={setDetailsDialogOpen}
          clientId={selectedClientId}
        />
      )}
    </div>
  );
}