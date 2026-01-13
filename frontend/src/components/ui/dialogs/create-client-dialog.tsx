import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"; // shadcn/ui
import { useState } from "react";
import { useCreateClient } from "@/hooks/useCreateClient";

import type { LegalTypeApi } from "@/types/legaltypeapi";
import type { RiskLevelApi } from "@/types/alert";
import type { KycStatusApi } from "@/types/kycstatus";

// Labels para exibição no Select (map API value => label)
const LEGAL_TYPE_OPTIONS: { value: LegalTypeApi; label: string }[] = [
  { value: "individual", label: "Individual" },
  { value: "company", label: "Empresa" },
];

const RISK_LEVEL_OPTIONS: { value: RiskLevelApi; label: string }[] = [
  { value: 0, label: "Baixo" },
  { value: 1, label: "Médio" },
  { value: 2, label: "Alto" },
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

  const [contactNumber, setContactNumber] = useState("");
  const [countryCode, setCountryCode] = useState("");

  async function handleSubmit() {
    if (!name || legalType === "" || riskLevel === "" || kycStatus === "" || !contactNumber || !countryCode) {
      alert("Preencha todos os campos obrigatórios");
      return;
    }

    await submit({
      name,
      legalType,
      contactNumber,
      countryCode,
      riskLevel,
      kycStatus,
    });

    onOpenChange(false);
    onSuccess();
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Criar Cliente</DialogTitle>
        </DialogHeader>

        <div className="space-y-3">
          <Input
            placeholder="Nome"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />

          {/* Legal Type */}
          <Select
            value={legalType}
            onValueChange={(value) => setLegalType(value as LegalTypeApi)}
          >
            <SelectTrigger>
              <SelectValue placeholder="Tipo Legal" />
            </SelectTrigger>
            <SelectContent>
              {LEGAL_TYPE_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          
          {/* Risk Level */}
          <Select
            value={riskLevel !== "" ? riskLevel.toString() : ""}
            onValueChange={(value) => setRiskLevel(Number(value) as RiskLevelApi)}
          >
            <SelectTrigger>
              <SelectValue placeholder="Nível de Risco" />
            </SelectTrigger>
            <SelectContent>
              {RISK_LEVEL_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value.toString()}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          {/* KYC Status */}
          <Select
            value={kycStatus !== "" ? kycStatus.toString() : ""}
            onValueChange={(value) => setKycStatus(Number(value) as KycStatusApi)}
          >
            <SelectTrigger>
              <SelectValue placeholder="Status KYC" />
            </SelectTrigger>
            <SelectContent>
              {KYC_STATUS_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value.toString()}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>


          <Input
            placeholder="Telefone"
            value={contactNumber}
            onChange={(e) => setContactNumber(e.target.value)}
          />
          <Input
            placeholder="País"
            value={countryCode}
            onChange={(e) => setCountryCode(e.target.value)}
          />

          {error && <p className="text-sm text-red-500">{error}</p>}
        </div>

        <DialogFooter>
          <Button onClick={handleSubmit} disabled={loading}>
            {loading ? "Criando..." : "Criar"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
