// src/components/clients/ClientFormDialog.tsx
import SimpleDialog from "@/components/ui/simple-dialog";
import { ClientForm, type CreateClientDTO } from "./ClientForm";

interface ClientFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (data: CreateClientDTO) => void;
  isLoading?: boolean;
}

export function ClientFormDialog({
  open,
  onOpenChange,
  onSubmit,
  isLoading,
}: ClientFormDialogProps) {
  return (
    <SimpleDialog
      open={open}
      onOpenChange={onOpenChange}
      title="Criar Novo Cliente"
      description="Preencha os dados do cliente para cadastro no sistema. Campos marcados com * são obrigatórios."
    >
      <ClientForm
        onSubmit={onSubmit}
        onCancel={() => onOpenChange(false)}
        isLoading={isLoading}
      />
    </SimpleDialog>
  );
}
