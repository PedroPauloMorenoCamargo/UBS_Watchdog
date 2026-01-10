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

// ===== ENUMS (fonte única da verdade) =====
export const RISK_LEVELS = ["low", "medium", "high"] as const;
export const KYC_STATUSES = ["pending", "verified", "expired"] as const;

// ===== TIPOS DERIVADOS =====
export type RiskLevel = (typeof RISK_LEVELS)[number];
export type KYCStatus = (typeof KYC_STATUSES)[number];

// ===== VALIDAÇÃO COM ZOD =====
export const clientSchema = z.object({
  name: z.string().min(3, "Nome deve ter no mínimo 3 caracteres"),
  country: z.string().min(1, "País é obrigatório"),

  risk: z.enum(RISK_LEVELS, {
    message: "Nível de risco é obrigatório",
  }),

  kyc: z.enum(KYC_STATUSES, {
    message: "Status KYC é obrigatório",
  }),
  email: z.string().email("Email inválido").optional().or(z.literal("")),
  phone: z.string().optional(),
  document: z.string().optional(),
});

export type CreateClientDTO = z.infer<typeof clientSchema>;

// ===== PROPS DO COMPONENTE =====
interface ClientFormProps {
  onSubmit: (data: CreateClientDTO) => void;
  onCancel: () => void;
  isLoading?: boolean;
  defaultValues?: Partial<CreateClientDTO>;
}

// ===== COMPONENTE =====
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
  } = useForm<CreateClientDTO>({
    resolver: zodResolver(clientSchema),
    defaultValues: {
      name: defaultValues?.name || "",
      country: defaultValues?.country || "",
      risk: defaultValues?.risk || "low",
      kyc: defaultValues?.kyc || "pending",
      email: defaultValues?.email || "",
      phone: defaultValues?.phone || "",
      document: defaultValues?.document || "",
    },
  });

  const risk = watch("risk");
  const kyc = watch("kyc");
  const country = watch("country");

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {/* ===== NOME ===== */}
      <div className="space-y-2">
        <Label htmlFor="name">
          Nome Completo <span className="text-red-500">*</span>
        </Label>
        <Input
          id="name"
          placeholder="Ex: João Silva"
          {...register("name")}
          disabled={isLoading}
        />
        {errors.name && (
          <p className="text-sm text-red-500">{errors.name.message}</p>
        )}
      </div>

      {/* ===== PAÍS ===== */}
      <div className="space-y-2">
        <Label htmlFor="country">
          País <span className="text-red-500">*</span>
        </Label>
        <Select
          value={country}
          onValueChange={(value) => setValue("country", value)}
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
        {errors.country && (
          <p className="text-sm text-red-500">{errors.country.message}</p>
        )}
      </div>

      {/* ===== GRID: RISCO + KYC ===== */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        {/* NÍVEL DE RISCO */}
        <div className="space-y-2">
          <Label htmlFor="risk">
            Nível de Risco <span className="text-red-500">*</span>
          </Label>
          <Select
            value={risk}
            onValueChange={(value) => setValue("risk", value as RiskLevel)}
            disabled={isLoading}
          >
            <SelectTrigger>
              <SelectValue placeholder="Selecione o nível de risco" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="low">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-green-500" />
                  Baixo (Low)
                </span>
              </SelectItem>
              <SelectItem value="medium">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-yellow-500" />
                  Médio (Medium)
                </span>
              </SelectItem>
              <SelectItem value="high">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-red-500" />
                  Alto (High)
                </span>
              </SelectItem>
            </SelectContent>
          </Select>
          {errors.risk && (
            <p className="text-sm text-red-500">{errors.risk.message}</p>
          )}
        </div>

        {/* KYC STATUS */}
        <div className="space-y-2">
          <Label htmlFor="kyc">
            Status KYC <span className="text-red-500">*</span>
          </Label>
          <Select
            value={kyc}
            onValueChange={(value) => setValue("kyc", value as KYCStatus)}
            disabled={isLoading}
          >
            <SelectTrigger>
              <SelectValue placeholder="Selecione o status KYC" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="pending">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-yellow-500" />
                  Pendente (Pending)
                </span>
              </SelectItem>
              <SelectItem value="verified">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-green-500" />
                  Verificado (Verified)
                </span>
              </SelectItem>
              <SelectItem value="expired">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-red-500" />
                  Expirado (Expired)
                </span>
              </SelectItem>
            </SelectContent>
          </Select>
          {errors.kyc && (
            <p className="text-sm text-red-500">{errors.kyc.message}</p>
          )}
        </div>
      </div>

      {/* ===== SEPARADOR ===== */}
      <div className="border-t pt-4">
        <h3 className="mb-4 text-sm font-semibold text-slate-700">
          Informações Adicionais (Opcional)
        </h3>

        {/* EMAIL */}
        <div className="mb-4 space-y-2">
          <Label htmlFor="email">Email</Label>
          <Input
            id="email"
            type="email"
            placeholder="cliente@exemplo.com"
            {...register("email")}
            disabled={isLoading}
          />
          {errors.email && (
            <p className="text-sm text-red-500">{errors.email.message}</p>
          )}
        </div>

        {/* GRID: TELEFONE + DOCUMENTO */}
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          {/* TELEFONE */}
          <div className="space-y-2">
            <Label htmlFor="phone">Telefone</Label>
            <Input
              id="phone"
              placeholder="+55 11 99999-9999"
              {...register("phone")}
              disabled={isLoading}
            />
          </div>

          {/* DOCUMENTO */}
          <div className="space-y-2">
            <Label htmlFor="document">Documento (CPF/Passaporte)</Label>
            <Input
              id="document"
              placeholder="000.000.000-00"
              {...register("document")}
              disabled={isLoading}
            />
          </div>
        </div>
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