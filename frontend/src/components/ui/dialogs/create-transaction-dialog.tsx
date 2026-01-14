"use client";
import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { api } from "@/lib/api";
import type { TransactionResponseDto, TransactionTypeApi, TransferMethodApi } from "@/types/Transactions/transaction";

interface Props {
  isOpen: boolean;
  onClose: () => void;
  accountId: string;
  onCreated?: (tx: TransactionResponseDto) => void;
}

export function CreateTransactionDialog({ isOpen, onClose, accountId, onCreated }: Props) {
  const [type, setType] = useState<TransactionTypeApi>(0);
  const [transferMethod, setTransferMethod] = useState<TransferMethodApi | null>(null);
  const [amount, setAmount] = useState<number>(0);
  const [currency, setCurrency] = useState("USD");

  const [cpName, setCpName] = useState("");
  const [cpBank, setCpBank] = useState("");
  const [cpBranch, setCpBranch] = useState("");
  const [cpAccount, setCpAccount] = useState("");
  const [cpIdentifierType, setCpIdentifierType] = useState<string | undefined>("none");
  const [cpIdentifier, setCpIdentifier] = useState("");
  const [cpCountryCode, setCpCountryCode] = useState("");

  const [loading, setLoading] = useState(false);

  const handleSubmit = async () => {
    setLoading(true);
    try {
      if (!accountId) throw new Error("AccountId is required");
      if (amount <= 0) throw new Error("Amount must be greater than 0");
      if (!currency) throw new Error("CurrencyCode is required");

      const payload = {
        accountId,
        type,
        amount,
        currencyCode: currency,
        occurredAtUtc: new Date().toISOString(),
        transferMethod: transferMethod ?? undefined,
        cpName: cpName || undefined,
        cpBank: cpBank || undefined,
        cpBranch: cpBranch || undefined,
        cpAccount: cpAccount || undefined,
        cpIdentifierType:
          cpIdentifierType && cpIdentifierType !== "none"
            ? Number(cpIdentifierType)
            : undefined,
        cpIdentifier: cpIdentifier || undefined,
        cpCountryCode: cpCountryCode || undefined,
      };

      console.log("Payload enviado:", payload);

      const res = await api.post<TransactionResponseDto>("/api/transactions", payload);

      console.log("Transaction criada:", res.data);

      onCreated?.(res.data);
      onClose();
    } catch (err) {
      console.error(err);
      alert("Error creating transaction: " + (err as any).message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-3xl">
        <DialogHeader>
          <DialogTitle>Create Transaction</DialogTitle>
        </DialogHeader>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 py-4">
          <div>
            <label className="text-xs font-medium text-slate-500">Type</label>
            <Select value={type.toString()} onValueChange={(v) => setType(Number(v) as TransactionTypeApi)}>
              <SelectTrigger>
                <SelectValue placeholder="Select type" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="0">Deposit</SelectItem>
                <SelectItem value="1">Withdrawal</SelectItem>
                <SelectItem value="2">Wire</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Transfer Method</label>
            <Select
              value={transferMethod !== null ? transferMethod.toString() : "none"}
              onValueChange={(v) =>
                setTransferMethod(v === "none" ? null : (Number(v) as TransferMethodApi))
              }
            >
              <SelectTrigger>
                <SelectValue placeholder="Select method" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="none">None</SelectItem>
                <SelectItem value="0">Bank</SelectItem>
                <SelectItem value="1">Cash</SelectItem>
                <SelectItem value="2">PIX</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Amount</label>
            <Input type="number" value={amount} onChange={(e) => setAmount(Number(e.target.value))} />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Currency</label>
            <Input value={currency} onChange={(e) => setCurrency(e.target.value)} />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Counterparty Name</label>
            <Input value={cpName} onChange={(e) => setCpName(e.target.value)} />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Bank</label>
            <Input value={cpBank} onChange={(e) => setCpBank(e.target.value)} />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Branch</label>
            <Input value={cpBranch} onChange={(e) => setCpBranch(e.target.value)} />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Account</label>
            <Input value={cpAccount} onChange={(e) => setCpAccount(e.target.value)} />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Identifier Type</label>
            <Select
              value={cpIdentifierType ?? "none"}
              onValueChange={(v) => setCpIdentifierType(v)}
            >
              <SelectTrigger><SelectValue placeholder="Select type" /></SelectTrigger>
              <SelectContent>
                <SelectItem value="none">None</SelectItem>
                <SelectItem value="0">CPF</SelectItem>
                <SelectItem value="1">CNPJ</SelectItem>
                <SelectItem value="2">TAX_ID</SelectItem>
                <SelectItem value="3">PASSPORT</SelectItem>
                <SelectItem value="4">LEI</SelectItem>
                <SelectItem value="5">PIX_EMAIL</SelectItem>
                <SelectItem value="6">PIX_PHONE</SelectItem>
                <SelectItem value="7">PIX_RANDOM</SelectItem>
                <SelectItem value="8">IBAN</SelectItem>
                <SelectItem value="9">OTHER</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Identifier</label>
            <Input value={cpIdentifier} onChange={(e) => setCpIdentifier(e.target.value)} />
          </div>

          <div>
            <label className="text-xs font-medium text-slate-500">Country Code</label>
            <Input value={cpCountryCode} onChange={(e) => setCpCountryCode(e.target.value)} />
          </div>
        </div>

        <DialogFooter className="flex justify-end gap-2">
          <Button variant="outline" onClick={onClose}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={loading}>
            {loading ? "Creating..." : "Create"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
