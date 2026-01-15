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
         legalType: defaultValues?.legalType === 0 ? "individual" : defaultValues?.legalType === 1 ? "company" : "individual",
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
          legalType: (data.legalType === "individual" ? 0 : 1) as LegalTypeApi,
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
            <SelectValue placeholder="Select type" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="individual">Individual</SelectItem>
            <SelectItem value="company">Company</SelectItem>
          </SelectContent>
        </Select>
        {errors.legalType && (
          <p className="text-sm text-red-500">{errors.legalType.message}</p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="name">
          Name <span className="text-red-500">*</span>
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
      <div className="space-y-2">
        <Label htmlFor="contactNumber">Telefone</Label>
        <Input
          id="contactNumber"
          placeholder="+55 11 99999-9999"
          {...register("contactNumber")}
          disabled={isLoading}
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="countryCode">
          Country Code <span className="text-red-500">*</span>
        </Label>
        <Select
          value={countryCode}
          onValueChange={(value) => setValue("countryCode", value)}
          disabled={isLoading}
        >
          <SelectTrigger>
            <SelectValue placeholder="Select country" />
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

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="riskLevel">
            Risk Level <span className="text-red-500">*</span>
          </Label>
          <Select
            value={String(riskLevel)}
            onValueChange={(value) => setValue("riskLevel", Number(value) as RiskLevelApi)}
            disabled={isLoading}
          >
            <SelectTrigger>
              <SelectValue placeholder="Select risk level" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="0">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-green-500" />
                  Low
                </span>
              </SelectItem>
              <SelectItem value="1">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-yellow-500" />
                  Medium
                </span>
              </SelectItem>
              <SelectItem value="2">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-red-500" />
                 High
                </span>
              </SelectItem>
            </SelectContent>
          </Select>
          {errors.riskLevel && (
            <p className="text-sm text-red-500">{errors.riskLevel.message}</p>
          )}
        </div>

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
                  Pending
                </span>
              </SelectItem>
              <SelectItem value="1">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-green-500" />
                  Verified
                </span>
              </SelectItem>
              <SelectItem value="2">
                <span className="flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-red-500" />
                  Expired
                </span>
              </SelectItem>
            </SelectContent>
          </Select>
          {errors.kycStatus && (
            <p className="text-sm text-red-500">{errors.kycStatus.message}</p>
          )}
        </div>
      </div>

      <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
        <p className="text-sm font-medium text-slate-700">Information</p>
        <ul className="mt-2 space-y-1 text-xs text-slate-600">
          <li>• The data will be validated by the compliance system</li>
          <li>• High-risk clients require additional approval</li>
          <li>• The KYC status can be updated later</li>
        </ul>
      </div>
      <div className="flex justify-end gap-3 border-t pt-4">
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={isLoading}
        >
          Cancel
        </Button>
        <Button type="submit" disabled={isLoading}>
          {isLoading ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Saving...
            </>
          ) : (
            "Create client"
          )}
        </Button>
      </div>
    </form>
  );
}