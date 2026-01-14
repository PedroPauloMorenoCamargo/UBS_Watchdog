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
import { createAccountIdentifier } from "@/services/accountIdentifiers.service";
import { IdentifierType, identifierTypeMap } from "@/types/AccountIdentifiers/accountIdentifier";
import { useCountries } from "@/hooks/useCountries";
import { Loader2 } from "lucide-react";

interface CreateIdentifierDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accountId: string;
  onSuccess?: () => void;
}

export function CreateIdentifierDialog({
  open,
  onOpenChange,
  accountId,
  onSuccess,
}: CreateIdentifierDialogProps) {
  const [identifierType, setIdentifierType] = useState<IdentifierType | "">("");
  const [identifierValue, setIdentifierValue] = useState("");
  const [issuedCountryCode, setIssuedCountryCode] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { countries: countryList, loading: countriesLoading } = useCountries();

  const resetForm = () => {
    setIdentifierType("");
    setIdentifierValue("");
    setIssuedCountryCode("");
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
    
    if (identifierType === "" || !identifierValue) {
      setError("Identifier type and value are required");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      await createAccountIdentifier(accountId, {
        identifierType: identifierType as IdentifierType,
        identifierValue,
        issuedCountryCode: issuedCountryCode || null,
      });
      
      onSuccess?.();
      handleClose(false);
    } catch (err: unknown) {
      console.error("Failed to create identifier:", err);
      const errorMessage = err instanceof Error 
        ? err.message 
        : (err as { response?: { data?: { detail?: string } } })?.response?.data?.detail || "Failed to create identifier";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Add Account Identifier</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="identifierType">Identifier Type *</Label>
            <Select
              value={identifierType === "" ? undefined : String(identifierType)}
              onValueChange={(value) => setIdentifierType(Number(value) as IdentifierType)}
              disabled={loading}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select identifier type" />
              </SelectTrigger>
              <SelectContent position="popper" className="max-h-[200px] overflow-y-auto">
                {Object.entries(identifierTypeMap).map(([key, label]) => (
                  <SelectItem key={key} value={key}>
                    {label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="identifierValue">Identifier Value *</Label>
            <Input
              id="identifierValue"
              placeholder="e.g., 123.456.789-00"
              value={identifierValue}
              onChange={(e) => setIdentifierValue(e.target.value)}
              disabled={loading}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="country">Issued Country (Optional)</Label>
            <Select
              value={issuedCountryCode || undefined}
              onValueChange={setIssuedCountryCode}
              disabled={loading || countriesLoading}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select country (optional)" />
              </SelectTrigger>
              <SelectContent position="popper" className="max-h-[200px] overflow-y-auto">
                {countryList.map((c) => (
                  <SelectItem key={c.code} value={c.code}>
                    {c.name}
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
              Add Identifier
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
