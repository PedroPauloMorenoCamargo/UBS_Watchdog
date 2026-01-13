// src/components/clients/ClientForm.tsx
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
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
import { COUNTRIES } from "@/types/countries";
import { Loader2 } from "lucide-react";
import type { RiskLevelApi } from "@/types/alert";
import type { KycStatusApi } from "@/types/kycstatus";
import type { LegalTypeApi } from "@/types/legaltypeapi";

export interface CreateClientFormData {
  legalType: LegalTypeApi;
  name: string;
  contactNumber?: string;
  countryCode: string;
  riskLevel: RiskLevelApi;
  kycStatus: KycStatusApi;
}

const clientSchema = z.object({
  legalType: z.enum(["individual", "company"], {
    message: "Tipo legal é obrigatório",
  }),
  name: z.string().min(3, "Nome deve ter no mínimo 3 caracteres"),
  contactNumber: z.string().optional(),
  countryCode: z.string().min(2, "País é obrigatório"),
  riskLevel: z.number().min(0).max(2, "Nível de risco inválido"),
  kycStatus: z.number().min(0).max(2, "Status KYC inválido"),
});

type ClientFormData = z.infer<typeof clientSchema>;

interface ClientFormProps {
  onSubmit: (data: CreateClientFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
  defaultValues?: Partial<CreateClientFormData>;
}

const riskLevelMap: Record<string, RiskLevelApi> = {
  low: 0,
  medium: 1,
  high: 2,
};

const kycStatusMap: Record<string, KycStatusApi> = {
  pending: 0,
  verified: 1,
  expired: 2,
};

export function ClientForm({
  onSubmit,
  onCancel,
  isLoading = false,
  defaultValues,
}: ClientFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
  } = useForm<ClientFormData>({
    resolver: zodResolver(clientSchema),
    defaultValues: {
      legalType: defaultValues?.legalType || "individual",
      name: defaultValues?.name || "",
      contactNumber: defaultValues?.contactNumber || "",
      countryCode: defaultValues?.countryCode || "",
      riskLevel: defaultValues?.riskLevel ?? 0,
      kycStatus: defaultValues?.kycStatus ?? 0,
    },
  });

  const legalType = watch("legalType");
  const riskLevel = watch("riskLevel");
  const kycStatus = watch("kycStatus");
  const countryCode = watch("countryCode");

  return (
    <form
        onSubmit={handleSubmit((data) => {
          const apiData: CreateClientFormData = {
            legalType: data.legalType,
            name: data.name,
            contactNumber: data.contactNumber,
            countryCode: data.countryCode,
            riskLevel: data.riskLevel as RiskLevelApi,
            kycStatus: data.kycStatus as KycStatusApi,
          };

          onSubmit(apiData);
        })}
        className="space-y-6"
      >

      {/* ===== TIPO LEGAL ===== */}
      <div className="space-y-2">
        <Label htmlFor="legalType">
          Tipo <span className="text-red-500">*</span>
        </Label>
        <Select
          value={legalType}
          onValueChange={(value) => setValue("legalType", value as "individual" | "company")}
          disabled={isLoading}
        >
          <SelectTrigger>
            <SelectValue placeholder="Selecione o tipo" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="individual">Pessoa Física</SelectItem>
            <SelectItem value="company">Pessoa Jurídica</SelectItem>
          </SelectContent>
        </Select>
        {errors.legalType && (
          <p className="text-sm text-red-500">{errors.legalType.message}</p>
        )}
      </div>

      {/* ===== NOME ===== */}
      <div className="space-y-2">
        <Label htmlFor="name">
          Nome Completo <span className="text-red-500">*</span>
        </Label>
        <Input
          id="name"
          placeholder="Ex: João Silva ou Empresa LTDA"
          {...register("name")}
          disabled={isLoading}
        />
        {errors.name && (
          <p className="text-sm text-red-500">{errors.name.message}</p>
        )}
      </div>

      {/* ===== TELEFONE ===== */}
      <div className="space-y-2">
        <Label htmlFor="contactNumber">Telefone</Label>
        <Input
          id="contactNumber"
          placeholder="+55 11 99999-9999"
          {...register("contactNumber")}
          disabled={isLoading}
        />
      </div>

      {/* ===== PAÍS ===== */}
      <div className="space-y-2">
        <Label htmlFor="countryCode">
          País <span className="text-red-500">*</span>
        </Label>
        <Select
          value={countryCode}
          onValueChange={(value) => setValue("countryCode", value)}
          disabled={isLoading}
        >
          <SelectTrigger>
            <SelectValue placeholder="Selecione o país" />
          </SelectTrigger>
          <SelectContent>
            {COUNTRIES.map((c) => (
              <SelectItem key={c} value={c}>
                {c}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        {errors.countryCode && (
          <p className="text-sm text-red-500">{errors.countryCode.message}</p>
        )}
      </div>

      {/* ===== GRID: RISCO + KYC ===== */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        {/* NÍVEL DE RISCO */}
        <div className="space-y-2">
          <Label htmlFor="riskLevel">
            Nível de Risco <span className="text-red-500">*</span>
          </Label>
          <Select
            value={String(riskLevel)}
            onValueChange={(value) => setValue("riskLevel", Number(value) as RiskLevelApi)}
            disabled={isLoading}
          >
            <SelectTrigger>
              <SelectValue placeholder="Selecione o nível de risco" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="0">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-green-500" />
                  Baixo (Low)
                </span>
              </SelectItem>
              <SelectItem value="1">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-yellow-500" />
                  Médio (Medium)
                </span>
              </SelectItem>
              <SelectItem value="2">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-red-500" />
                  Alto (High)
                </span>
              </SelectItem>
            </SelectContent>
          </Select>
          {errors.riskLevel && (
            <p className="text-sm text-red-500">{errors.riskLevel.message}</p>
          )}
        </div>

        {/* KYC STATUS */}
        <div className="space-y-2">
          <Label htmlFor="kycStatus">
            Status KYC <span className="text-red-500">*</span>
          </Label>
          <Select
            value={String(kycStatus)}
            onValueChange={(value) => setValue("kycStatus", Number(value) as KycStatusApi)}
            disabled={isLoading}
          >
            <SelectTrigger>
              <SelectValue placeholder="Selecione o status KYC" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="0">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-yellow-500" />
                  Pendente (Pending)
                </span>
              </SelectItem>
              <SelectItem value="1">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-green-500" />
                  Verificado (Verified)
                </span>
              </SelectItem>
              <SelectItem value="2">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-red-500" />
                  Expirado (Expired)
                </span>
              </SelectItem>
            </SelectContent>
          </Select>
          {errors.kycStatus && (
            <p className="text-sm text-red-500">{errors.kycStatus.message}</p>
          )}
        </div>
      </div>

      {/* ===== INFORMAÇÃO ===== */}
      <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
        <p className="text-sm font-medium text-slate-700">ℹ️ Informação</p>
        <ul className="mt-2 space-y-1 text-xs text-slate-600">
          <li>• Os dados serão validados pelo sistema de compliance</li>
          <li>• Clientes de alto risco requerem aprovação adicional</li>
          <li>• O status KYC pode ser atualizado posteriormente</li>
        </ul>
      </div>

      {/* ===== BOTÕES DE AÇÃO ===== */}
      <div className="flex justify-end gap-3 border-t pt-4">
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={isLoading}
        >
          Cancelar
        </Button>
        <Button type="submit" disabled={isLoading}>
          {isLoading ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Salvando...
            </>
          ) : (
            "Criar Cliente"
          )}
        </Button>
      </div>
    </form>
  );
}