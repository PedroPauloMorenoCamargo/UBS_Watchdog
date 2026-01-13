import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { useImportClientsCsv } from "@/hooks/useImportCsv";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess: () => void;
}

export function ImportClientsCsvDialog({
  open,
  onOpenChange,
  onSuccess,
}: Props) {
  const [file, setFile] = useState<File | null>(null);
  const { submit, loading, error } = useImportClientsCsv(() => {
    onOpenChange(false);
    onSuccess();
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Import Clients (CSV)</DialogTitle>
        </DialogHeader>

        <input
          type="file"
          accept=".csv"
          onChange={(e) => setFile(e.target.files?.[0] ?? null)}
        />

        {error && <p className="text-sm text-red-500">{error}</p>}

        <DialogFooter>
          <Button
            disabled={!file || loading}
            onClick={() => file && submit(file)}
          >
            {loading ? "Importing..." : "Import"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
