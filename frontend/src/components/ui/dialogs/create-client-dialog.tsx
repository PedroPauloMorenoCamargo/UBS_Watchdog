import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"; 
import { useState } from "react";
import { useCreateClient } from "@/hooks/useCreateClient";
import { useCountriesList } from "@/hooks/useCountriesList";


import type { LegalTypeApi } from "@/types/legaltypeapi";
import type { RiskLevelApi } from "@/types/alert";
import type { KycStatusApi } from "@/types/kycstatus";

// Options
const LEGAL_TYPE_OPTIONS: { value: LegalTypeApi; label: string }[] = [
  { value: 0, label: "Individual" },
  { value: 1, label: "Company" },
];

const RISK_LEVEL_OPTIONS: { value: RiskLevelApi; label: string }[] = [
  { value: 0, label: "Low" },
  { value: 1, label: "Medium" },
  { value: 2, label: "High" },
];

const KYC_STATUS_OPTIONS: { value: KycStatusApi; label: string }[] = [
  { value: 0, label: "Expired" },
  { value: 1, label: "Pending" },
  { value: 2, label: "Verified" },
  { value: 3, label: "Rejected" },
];

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => void;
}

export function CreateClientDialog({ open, onOpenChange, onSuccess }: Props) {
  const { submit, loading, error } = useCreateClient();

  const [name, setName] = useState("");
  const [legalType, setLegalType] = useState<LegalTypeApi | "">("");
  const [riskLevel, setRiskLevel] = useState<RiskLevelApi | "">("");
  const [kycStatus, setKycStatus] = useState<KycStatusApi | "">("");
  const [countryCode, setCountryCode] = useState<string>("");
  const [telephone, setTelephone] = useState("");
  const [address, setAddress] = useState("");


  const { countries: countryList, loading: countriesLoading, error: countriesError } = useCountriesList();

  async function handleFormSubmit() {
    if (!name || legalType === "" || riskLevel === "" || kycStatus === "" || !countryCode || !telephone || !address) {
      alert("Please fill in all required fields.");
      return;
    }
    
    const result = await submit({
      legalType: legalType as LegalTypeApi,           
      name,
      contactNumber: telephone,
      addressJson: address,
      countryCode,
      initialRiskLevel: riskLevel as RiskLevelApi,   
      kycStatus: kycStatus as KycStatusApi,          
    });

    if (result.success) {
      // Reset form
      setName("");
      setLegalType("");
      setRiskLevel("");
      setKycStatus("");
      setCountryCode("");
      setTelephone("");
      setAddress("");
      
      onOpenChange(false);
      onSuccess();
    }
  }



  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange} modal>
        <DialogContent className="max-w-md max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Create New Client</DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            <div className="flex flex-col">
              <label className="text-sm font-medium text-slate-700">Full Name *</label>
              <Input
                placeholder="Ex: John Doe or Company LLC"
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
            </div>

            <div className="flex flex-col">
              <label className="text-sm font-medium text-slate-700">Type *</label>
              <Select
                value={legalType !== "" ? legalType.toString() : ""} 
                onValueChange={(value) => setLegalType(Number(value) as LegalTypeApi)} 
            >
              <SelectTrigger>
                <SelectValue placeholder="Select type" />
              </SelectTrigger>
              <SelectContent>
                {LEGAL_TYPE_OPTIONS.map((option) => (
                  <SelectItem key={option.value} value={option.value.toString()}> 
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>


          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col">
              <label className="text-sm font-medium text-slate-700">Risk Level *</label>
              <Select
                value={riskLevel !== "" ? riskLevel.toString() : ""}
                onValueChange={(value) => setRiskLevel(Number(value) as RiskLevelApi)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select risk" />
                </SelectTrigger>
                <SelectContent>
                  {RISK_LEVEL_OPTIONS.map((option) => (
                    <SelectItem key={option.value} value={option.value.toString()}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="flex flex-col">
              <label className="text-sm font-medium text-slate-700">KYC Status *</label>
              <Select
                value={kycStatus !== "" ? kycStatus.toString() : ""}
                onValueChange={(value) => setKycStatus(Number(value) as KycStatusApi)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select status" />
                </SelectTrigger>
                <SelectContent>
                  {KYC_STATUS_OPTIONS.map((option) => (
                    <SelectItem key={option.value} value={option.value.toString()}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="flex flex-col">
            <label className="text-sm font-medium text-slate-700">Country *</label>
            <Select
              value={countryCode}
              onValueChange={(value) => setCountryCode(value)}
              disabled={countriesLoading}
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

          <div className="flex flex-col">
            <label className="text-sm font-medium text-slate-700">Telephone *</label>
            <Input
              placeholder="+1 123-456-7890"
              value={telephone}
              onChange={(e) => setTelephone(e.target.value)}
            />
          </div>

          <div className="flex flex-col">
            <label className="text-sm font-medium text-slate-700">Address *</label>
            <Input
              placeholder="Street, City, State, ZIP"
              value={address}
              onChange={(e) => setAddress(e.target.value)}
            />
          </div>

          <div className="p-3 border rounded bg-blue-50 text-sm text-blue-700">
            <p>• Data will be validated by the compliance system</p>
            <p>• High-risk clients require additional approval</p>
            <p>• KYC status can be updated later</p>
          </div>

          {error && <p className="text-sm text-red-500">{error}</p>}
        </div>

        <DialogFooter className="flex justify-end gap-2">
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={loading}>
            Cancel
          </Button>
          <Button onClick={handleFormSubmit} disabled={loading}>
            {loading ? "Creating..." : "Create Client"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>


  </>
  );
}
