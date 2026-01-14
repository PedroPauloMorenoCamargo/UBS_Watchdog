"use client";

import { useState, useRef } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { api } from "@/lib/api";
import type { TransactionResponseDto } from "@/types/Transactions/transaction";

interface ImportTransactionsCsvDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onImported?: (transactions: TransactionResponseDto[]) => void;
}

export function ImportTransactionsCsvDialog({
  isOpen,
  onClose,
  onImported,
}: ImportTransactionsCsvDialogProps) {
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const inputRef = useRef<HTMLInputElement>(null);

  const handleImport = async () => {
    if (!file) {
      setError("Please select a CSV file");
      return;
    }

    setLoading(true);
    setError(null);

    const formData = new FormData();
    formData.append("file", file);

    try {
      const { data } = await api.post<TransactionResponseDto[]>(
        "/api/transactions/import",
        formData,
        { headers: { "Content-Type": "multipart/form-data" } }
      );

      onImported?.(data);
      setFile(null);
      onClose();
    } catch (err: any) {
      console.error("CSV import error:", err);
      const message =
        err?.response?.data?.title ||
        "Failed to import CSV. Check the file format and data.";
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFile(e.target.files?.[0] ?? null);
    setError(null);
  };

  const triggerFileSelect = () => inputRef.current?.click();

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Import Transactions CSV</DialogTitle>
        </DialogHeader>

        <div className="py-4 flex flex-col gap-3">
          <input
            type="file"
            accept=".csv"
            ref={inputRef}
            className="hidden"
            onChange={handleFileSelect}
          />

          <Button onClick={triggerFileSelect} disabled={loading}>
            {file ? "Change File" : "Choose File"}
          </Button>

          <p className="text-sm text-slate-600">
            {file ? file.name + ` (${(file.size / 1024).toFixed(2)} KB)` : "No file selected"}
          </p>

          {error && <p className="text-sm text-red-500">{error}</p>}
        </div>

        <DialogFooter className="flex justify-end gap-2">
          <Button variant="outline" onClick={onClose} disabled={loading}>
            Cancel
          </Button>
          <Button onClick={handleImport} disabled={!file || loading}>
            {loading ? "Importing..." : "Import"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
