// src/components/ui/clients/ClientFormDialog.tsx
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";

import { ClientForm, type CreateClientFormData } from "./ClientForm";

interface ClientFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (data: CreateClientFormData) => Promise<void> | void;
  isLoading?: boolean;
}

export function ClientFormDialog({
  open,
  onOpenChange,
  onSubmit,
  isLoading,
}: ClientFormDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>Create new client</DialogTitle>
          <DialogDescription>
            Fill in the client data to register in the system.
            Fields marked with * are mandatory.
          </DialogDescription>
        </DialogHeader>

        <ClientForm
          onSubmit={onSubmit}
          onCancel={() => onOpenChange(false)}
          isLoading={isLoading}
        />
      </DialogContent>
    </Dialog>
  );
}
