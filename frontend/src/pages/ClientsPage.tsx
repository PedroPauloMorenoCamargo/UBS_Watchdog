import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useMemo, useState, useRef } from "react";
import type { SeverityFilter } from "@/types/alert";
import type { KycFilter } from "@/types/kycstatus";
import { useCountries } from "@/hooks/useCountries";
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
import { ClientFormDialog } from "@/components/ui/clients/ClientFormDialog";
import { useCreateClient } from "@/hooks/useCreateClient";
import { useImportClientsCsv } from "@/hooks/useImportCsv";
import type { CreateClientFormData } from "@/components/ui/clients/ClientForm";
import type { CreateClientDto } from "@/types/Clients/client";
import { CreateClientDialog } from "@/components/ui/dialogs/create-client-dialog";


export function ClientsPage() {
  const [search, setSearch] = useState("");
  const [risk, setRisk] = useState<SeverityFilter>("all");
  const [countries, setCountries] = useState<string>("all");
  const [kyc, setKyc] = useState<KycFilter>("all");
  const [dialogOpen, setDialogOpen] = useState(false);

  const fileInputRef = useRef<HTMLInputElement | null>(null);

  const { data, loading, error } = useApi({
    fetcher: fetchClients,
  });

  const {
    submit: createNewClient,
    loading: isCreating,
  } = useCreateClient();

  const {
    submit: importCsv,
    loading: isImporting,
  } = useImportClientsCsv();

  const { countries: countryList, loading: countriesLoading, error: countriesError } = useCountries();

  const clients = useMemo(() => {
    if (!data) return [];
    return data.items.map(mapClientToTableRow);
  }, [data]);

  const filteredClients = useMemo(() => {
    return clients.filter((t) => {
      const searchMatch =
        !search || t.name.toLowerCase().includes(search.toLowerCase());

      const riskMatch = risk === "all" || t.risk.toLowerCase() === risk;

      const countryMatch = countries === "all" || t.country === countries;

      const kycMatch = kyc === "all" || t.kyc.toLowerCase() === kyc;

      return searchMatch && riskMatch && countryMatch && kycMatch;
    });
  }, [clients, search, risk, countries, kyc]);

  function handleCountryChange(value: string) {
    setCountries(value);
  }

  async function handleCreateClient(formData: CreateClientFormData) {
    const clientDto: CreateClientDto = {
      legalType: formData.legalType,
      name: formData.name,
      contactNumber: formData.contactNumber,
      countryCode: formData.countryCode,
      riskLevel: formData.riskLevel,
      kycStatus: formData.kycStatus,
      addressJson: {},
    };

    const result = await createNewClient(clientDto);

    if (result.success) {
      alert("Client created successfully!");
      setDialogOpen(false);
      window.location.reload();
    } else {
      alert(result.error ?? "Error creating client");
    }
  }


  async function handleImportCSV(event: React.ChangeEvent<HTMLInputElement>) {
  const file = event.target.files?.[0];
  if (!file) return;

  if (!file.name.toLowerCase().endsWith(".csv")) {
    alert("Only CSV files are allowed!");
    event.target.value = ""; 
    return;
  }

  await importCsv(file);

  alert("CSV imported successfully!");
  window.location.reload();

  event.target.value = "";
}
  

  return (
    <div className="relative w-full max-w-full overflow-x-hidden">
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
            disabled={isCreating}
          >
            {isCreating ? "Creating..." : "Create Client"}
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
          {loading && <p>Loading...</p>}
          {error && <p className="text-red-500">{error}</p>}
          {!loading && !error && <ClientsTable clients={filteredClients} />}
        </ChartCard>
      </div>

      <CreateClientDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        onSuccess={() => {
          alert("Client created successfully!");
          setDialogOpen(false);
          }}
      />
    </div>
  );
}