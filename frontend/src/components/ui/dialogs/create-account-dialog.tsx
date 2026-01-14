import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useState } from "react";
import { createClientAccount } from "@/services/accounts.service";
import { AccountType, accountTypeMap } from "@/types/Accounts/account";
import { Loader2 } from "lucide-react";
import { useCountriesList } from "@/hooks/useCountriesList";

interface CreateAccountDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  clientId: string;
  clientCountryCode?: string;
  onSuccess?: () => void;
}

const currencies = [
  { code: "USD", name: "US Dollar" },
  { code: "EUR", name: "Euro" },
  { code: "GBP", name: "British Pound" },
  { code: "CHF", name: "Swiss Franc" },
  { code: "BRL", name: "Brazilian Real" },
  { code: "JPY", name: "Japanese Yen" },
  { code: "SGD", name: "Singapore Dollar" },
];

export function CreateAccountDialog({
  open,
  onOpenChange,
  clientId,
  clientCountryCode,
  onSuccess,
}: CreateAccountDialogProps) {
  const [accountIdentifier, setAccountIdentifier] = useState("");
  const [countryCode, setCountryCode] = useState(clientCountryCode || "");
  const [accountType, setAccountType] = useState<AccountType | "">("");
  const [currencyCode, setCurrencyCode] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const { countries: countryList, loading: countriesLoading, error: countriesError } = useCountriesList();

  const resetForm = () => {
    setAccountIdentifier("");
    setCountryCode(clientCountryCode || "");
    setAccountType("");
    setCurrencyCode("");
    setError(null);
  };

  const handleClose = (isOpen: boolean) => {
    if (!isOpen) {
      resetForm();
    }
    onOpenChange(isOpen);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!accountIdentifier || !countryCode || accountType === "" || !currencyCode) {
      setError("All fields are required");
      return;
    }

    setLoading(true);
    setError(null);

    const requestData = {
      accountIdentifier,
      countryCode,
      accountType: accountType as AccountType,
      currencyCode,
    };
    
    console.log("Creating account with data:", requestData);

    try {
      await createClientAccount(clientId, requestData);
      
      onSuccess?.();
      handleClose(false);
    } catch (err: unknown) {
      console.error("Failed to create account:", err);
      const errorMessage = err instanceof Error 
        ? err.message 
        : (err as { response?: { data?: { detail?: string } } })?.response?.data?.detail || "Failed to create account";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Create New Account</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="accountIdentifier">Account Identifier *</Label>
            <Input
              id="accountIdentifier"
              placeholder="e.g., ACC-001234"
              value={accountIdentifier}
              onChange={(e) => setAccountIdentifier(e.target.value)}
              disabled={loading}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="accountType">Account Type *</Label>
            <Select
              value={accountType === "" ? undefined : String(accountType)}
              onValueChange={(value) => setAccountType(Number(value) as AccountType)}
              disabled={loading}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select account type" />
              </SelectTrigger>
              <SelectContent>
                {Object.entries(accountTypeMap).map(([key, label]) => (
                  <SelectItem key={key} value={key}>
                    {label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="country">Country *</Label>
            <Select
              value={countryCode}
              onValueChange={setCountryCode}
              disabled={loading || countriesLoading}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select country" />
              </SelectTrigger>
              <SelectContent position="popper" className="max-h-[200px] overflow-y-auto">
                {countryList.map((c: { code: string; name: string }) => (
                  <SelectItem key={c.code} value={c.code}>
                    {c.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {countriesError && <p className="text-red-500 text-xs">{countriesError}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="currency">Currency *</Label>
            <Select
              value={currencyCode}
              onValueChange={setCurrencyCode}
              disabled={loading}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select currency" />
              </SelectTrigger>
              <SelectContent>
                {currencies.map((currency) => (
                  <SelectItem key={currency.code} value={currency.code}>
                    {currency.name} ({currency.code})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {error && (
            <div className="text-sm text-red-500 bg-red-50 p-2 rounded">
              {error}
            </div>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => handleClose(false)}
              disabled={loading}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={loading}>
              {loading && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
              Create Account
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
