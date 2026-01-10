// Shadcn/ui
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useMemo,useState } from "react";
import type { SeverityFilter } from "@/types/alert";
import type { KycFilter } from "@/types/kycstatus";
import { COUNTRIES, type CountriesFilter } from "@/types/countries";

import { clientsMock } from "@/mocks/mocks";

import { ClientFormDialog } from "@/components/ui/clients/ClientFormDialog";
import type { CreateClientDTO } from "@/components/ui/clients/ClientForm";

import { Search } from "lucide-react";

import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";

import { ChartCard } from "@/components/ui/charts/chartcard";
import { ClientsTable } from "@/components/ui/tables/clientstable";



export function ClientsPage() {
  const [search, setSearch] = useState("");
  const [risk, setRisk] = useState<SeverityFilter>("all");
  const [countries, setCountries] = useState<CountriesFilter>("all");
  const [kyc, setKyc] = useState<KycFilter>("all");

  const [dialogOpen, setDialogOpen] = useState(false); 
  const [isCreating, setIsCreating] = useState(false);


function handleCountryChange(value: CountriesFilter) {
  setCountries(value);
}


  const filteredClients = useMemo(() => {
  return clientsMock.filter((t) => {

      const searchMatch =
        !search ||
        t.name.toLowerCase().includes(search.toLowerCase())

      
      const riskMatch =
        risk === "all" || t.risk.toLowerCase() === risk;

      const countryMatch =
      countries === "all" || t.country === countries;

      const kycMatch =
      kyc === "all" || t.kyc.toLowerCase() === kyc;

    return searchMatch && riskMatch && countryMatch && kycMatch;
  });
}, [search, risk, countries, kyc]);

const handleCreateClient = async (data: CreateClientDTO) => {
    setIsCreating(true);

    try {
      // 🔹 AQUI VOCÊ VAI CHAMAR A API OU REDUX
      console.log("Dados do cliente:", data);

      // Simulando delay de API
      await new Promise((resolve) => setTimeout(resolve, 1000));

      // TODO: Implementar chamada real
      // await dispatch(createClient(data)).unwrap();

      alert("Cliente criado com sucesso!");
      setDialogOpen(false); // Fecha o dialog após sucesso
    } catch (error) {
      console.error("Erro ao criar cliente:", error);
      alert("Erro ao criar cliente. Tente novamente.");
    } finally {
      setIsCreating(false);
    }
  };

  return (
    <div className="relative">
      <div className="mt-4 rounded-xl bg-white p-5 shadow">
        
        <div
          className="grid items-end gap-4"
          style={{ gridTemplateColumns: "2fr 1.5fr 1.5fr 1.5fr auto" }}
        >
          <div>
            <label className="text-xs font-medium text-slate-500">
              Search Client
            </label>
                <div className="relative">
                    <Search
                        className="absolute left-0 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400"
                    />
                
                    <Input
                    placeholder="Search by client name..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    className="h-9 rounded-none border-0 border-b border-slate-300 pl-6 pr-0 shadow-none focus-visible:ring-0"
                    />
                </div>
            </div>

          <div>
            <label className="text-xs font-medium text-slate-500">
              Risk
            </label>
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

            <Select value={countries} onValueChange={handleCountryChange}>
              <SelectTrigger className="h-9 w-full">
                <SelectValue placeholder="All" />
              </SelectTrigger>

              <SelectContent>
                <SelectItem value="all">All</SelectItem>

                {COUNTRIES.map((country) => (
                  <SelectItem key={country} value={country}>
                    {country}
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
            <Button variant="outline"
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
      
      <div className="mt-4 rounded-xl bg-white p-4 shadow">
          <div className="flex flex-wrap items-center gap-3">
            {/* <Button className="cursor-pointer hover:bg-slate-600"
              onClick={() => console.log("Criar")}>
              Criar Cliente
            </Button> */}

            <Button
              className="cursor-pointer hover:bg-slate-600"
              onClick={() => setDialogOpen(true)}
            >
              Criar Cliente
            </Button>

            <Button className="cursor-pointer hover:bg-slate-200"
              variant="outline" 
              onClick={() => console.log("Importar CSV")}>
              Importar CSV
            </Button>
          </div>
      </div>

      <div className="mt-5">
        <ChartCard title="Clients">
          <ClientsTable clients={filteredClients} />
        </ChartCard>
      </div>

      <ClientFormDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        onSubmit={handleCreateClient}
        isLoading={isCreating}
      />
    </div>
  );
}

    