import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { CountriesResponseDto } from "@/types/Countries/countries";
import type { Rule } from "@/types/rules";
import { Loader2, X } from "lucide-react";
import { fetchCountries} from "@/services/countries.service";

interface Props {
  open: boolean;
  rule: Rule | null;
  onOpenChange: (open: boolean) => void;
  onSave: (id: string, data: {
    name?: string;
    isActive?: boolean;
    severity?: "Low" | "Medium" | "High" | "Critical";
    scope?: string;
    parameters?: Record<string, unknown>;
  }) => Promise<void>;
  loading?: boolean;
}

const SEVERITY_OPTIONS = [
  { value: "Low", label: "Low" },
  { value: "Medium", label: "Medium" },
  { value: "High", label: "High" },
  { value: "Critical", label: "Critical" },
] as const;

export function ConfigureRuleDialog({
  open,
  rule,
  onOpenChange,
  onSave,
  loading = false,
}: Props) {
  const [name, setName] = useState("");
  const [isActive, setIsActive] = useState(false);
  const [severity, setSeverity] = useState<"Low" | "Medium" | "High" | "Critical">("Medium");
  const [scope, setScope] = useState("");
  const [parameters, setParameters] = useState<Record<string, unknown>>({});
  
  const [countries, setCountries] = useState<CountriesResponseDto[]>([]);
  const [countriesLoading, setCountriesLoading] = useState(false);
  const [selectedCountries, setSelectedCountries] = useState<string[]>([]);

  useEffect(() => {
    if (open && rule?.code.toLowerCase().includes("banned_countries")) {
      setCountriesLoading(true);
      fetchCountries()
        .then((data) => setCountries(data))
        .catch((err) => console.error("Error loading countries:", err))
        .finally(() => setCountriesLoading(false));
    }
  }, [open, rule?.code]);

  useEffect(() => {
    if (rule) {
      setName(rule.name);
      setIsActive(rule.enabled);
      setSeverity(mapSeverityToApi(rule.severity));
      setScope(rule.scope ?? "global");
      setParameters(rule.parameters);
      
      if (rule.code.toLowerCase().includes("banned_countries") && rule.parameters.countries) {
        setSelectedCountries(rule.parameters.countries as string[]);
      } else {
        setSelectedCountries([]);
      }
    }
  }, [rule]);

  function mapSeverityToApi(sev: string): "Low" | "Medium" | "High" | "Critical" {
    const lower = sev.toLowerCase();
    switch (lower) {
      case "low": return "Low";
      case "medium": return "Medium";
      case "high": return "High";
      case "critical": return "Critical";
      default: return "Medium";
    }
  }

  async function handleSubmit() {
    if (!rule) return;

    // Build parameters with selected countries for banned_countries rule
    let finalParameters = parameters;
    if (rule.code.toLowerCase().includes("banned_countries")) {
      finalParameters = { ...parameters, countries: selectedCountries };
    }

    await onSave(rule.id, {
      name: name !== rule.name ? name : undefined,
      isActive: isActive !== rule.enabled ? isActive : undefined,
      severity: mapSeverityToApi(rule.severity) !== severity ? severity : undefined,
      scope: scope !== (rule.scope ?? "global") ? scope : undefined,
      parameters: JSON.stringify(finalParameters) !== JSON.stringify(rule.parameters) ? finalParameters : undefined,
    });
  }

  function handleAddCountry(countryCode: string) {
    if (!selectedCountries.includes(countryCode)) {
      setSelectedCountries([...selectedCountries, countryCode]);
    }
  }

  function handleRemoveCountry(countryCode: string) {
    setSelectedCountries(selectedCountries.filter((c) => c !== countryCode));
  }

  function getCountryName(code: string): string {
    const country = countries.find((c) => c.code === code);
    return country ? country.name : code;
  }

  function handleParameterChange(key: string, value: string) {
    setParameters((prev) => ({
      ...prev,
      [key]: isNaN(Number(value)) ? value : Number(value),
    }));
  }

  function handleArrayParameterChange(key: string, value: string) {
    const array = value.split(",").map((v) => v.trim()).filter(Boolean);
    setParameters((prev) => ({
      ...prev,
      [key]: array,
    }));
  }

  function getArrayValue(key: string): string {
    const val = parameters[key];
    if (Array.isArray(val)) {
      return val.join(", ");
    }
    return "";
  }

  function renderParametersFields() {
    if (!rule) return null;

    const code = rule.code.toLowerCase();

    if (code.includes("daily_limit")) {
      return (
        <div className="space-y-2">
          <Label htmlFor="limitBaseAmount">Daily Limit (USD)</Label>
          <Input
            id="limitBaseAmount"
            type="number"
            min={0}
            step={100}
            value={parameters.limitBaseAmount?.toString() ?? ""}
            onChange={(e) => handleParameterChange("limitBaseAmount", e.target.value)}
            placeholder="e.g., 10000"
          />
          <p className="text-xs text-gray-500">
            Maximum USD amount a client can transact per day.
          </p>
        </div>
      );
    }

    if (code.includes("banned_countries")) {
      return (
        <div className="space-y-4">
          <div className="space-y-2">
            <Label>Select Banned Countries</Label>
            {countriesLoading ? (
              <div className="flex items-center gap-2 text-sm text-gray-500">
                <Loader2 className="w-4 h-4 animate-spin" />
                Loading countries...
              </div>
            ) : (
              <Select onValueChange={handleAddCountry}>
                <SelectTrigger>
                  <SelectValue placeholder="Select a country to ban" />
                </SelectTrigger>
                <SelectContent position="popper" className="max-h-[200px] overflow-y-auto">
                  {countries
                    .filter((c) => !selectedCountries.includes(c.code))
                    .sort((a, b) => a.name.localeCompare(b.name))
                    .map((country) => (
                      <SelectItem key={country.code} value={country.code}>
                        {country.name} ({country.code})
                      </SelectItem>
                    ))}
                </SelectContent>
              </Select>
            )}
          </div>

          {selectedCountries.length > 0 && (
            <div className="space-y-2">
              <Label>Banned Countries ({selectedCountries.length})</Label>
              <div className="flex flex-wrap gap-2">
                {selectedCountries.map((code) => (
                  <span
                    key={code}
                    className="inline-flex items-center gap-1 px-2 py-1 text-sm bg-red-100 text-red-800 rounded-md"
                  >
                    {getCountryName(code)} ({code})
                    <button
                      type="button"
                      onClick={() => handleRemoveCountry(code)}
                      className="hover:bg-red-200 rounded-full p-0.5"
                    >
                      <X className="w-3 h-3" />
                    </button>
                  </span>
                ))}
              </div>
            </div>
          )}

          <div className="space-y-2">
            <Label>Parameters Preview (JSON)</Label>
            <pre className="w-full p-2 text-xs bg-gray-50 border rounded-md font-mono overflow-auto max-h-[100px]">
              {JSON.stringify({ countries: selectedCountries }, null, 2)}
            </pre>
          </div>
        </div>
      );
    }

    if (code.includes("banned_accounts")) {
      return (
        <div className="space-y-2">
          <Label>Parameters (JSON)</Label>
          <textarea
            className="w-full min-h-[120px] p-2 text-sm border rounded-md font-mono"
            value={JSON.stringify(parameters, null, 2)}
            onChange={(e) => {
              try {
                setParameters(JSON.parse(e.target.value));
              } catch {
              }
            }}
          />
          <p className="text-xs text-gray-500">
            Edit the list of banned account identifiers in JSON format.
          </p>
        </div>
      );
    }

    if (code.includes("structuring")) {
      return (
        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="n">Maximum Number of Transactions (n)</Label>
            <Input
              id="n"
              type="number"
              min={1}
              value={parameters.n?.toString() ?? ""}
              onChange={(e) => handleParameterChange("n", e.target.value)}
              placeholder="e.g., 5"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="xBaseAmount">Max Amount per Transaction - xBaseAmount (USD)</Label>
            <Input
              id="xBaseAmount"
              type="number"
              min={0}
              step={100}
              value={parameters.xBaseAmount?.toString() ?? ""}
              onChange={(e) => handleParameterChange("xBaseAmount", e.target.value)}
              placeholder="e.g., 2000"
            />
          </div>
          <p className="text-xs text-gray-500">
            Detects when a client makes multiple transactions below a threshold to avoid detection.
            Parameters: n = number of transactions, xBaseAmount = max amount per transaction.
          </p>
        </div>
      );
    }

    return (
      <div className="space-y-2">
        <Label>Parameters (JSON)</Label>
        <textarea
          className="w-full min-h-[100px] p-2 text-sm border rounded-md font-mono"
          value={JSON.stringify(parameters, null, 2)}
          onChange={(e) => {
            try {
              setParameters(JSON.parse(e.target.value));
            } catch {
            }
          }}
        />
        <p className="text-xs text-gray-500">
          Edit parameters in JSON format.
        </p>
      </div>
    );
  }

  if (!rule) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Configure Rule</DialogTitle>
          <DialogDescription>
            Adjust the parameters for rule "{rule.name}"
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6 py-4">
          <div className="space-y-2">
            <Label htmlFor="ruleName">Rule Name</Label>
            <Input
              id="ruleName"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Rule name"
            />
          </div>

          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label htmlFor="isActive">Active Rule</Label>
              <p className="text-xs text-gray-500">
                {isActive ? "Rule is active and will be evaluated" : "Rule is inactive"}
              </p>
            </div>
            <Switch
              id="isActive"
              checked={isActive}
              onCheckedChange={setIsActive}
            />
          </div>

          <div className="space-y-2">
            <Label>Severity</Label>
            <Select value={severity} onValueChange={(val) => setSeverity(val as typeof severity)}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {SEVERITY_OPTIONS.map((opt) => (
                  <SelectItem key={opt.value} value={opt.value}>
                    {opt.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="scope">Scope</Label>
            <Input
              id="scope"
              value={scope}
              onChange={(e) => setScope(e.target.value)}
              placeholder="global or country code (e.g., BR)"
            />
            <p className="text-xs text-gray-500">
              Use "global" to apply to all transactions, or a country code.
            </p>
          </div>

          <div className="border-t pt-4">
            <h4 className="font-medium mb-3">Rule Parameters</h4>
            {renderParametersFields()}
          </div>
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={loading}
          >
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={loading}>
            {loading ? (
              <>
                <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                Saving...
              </>
            ) : (
              "Save Changes"
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
