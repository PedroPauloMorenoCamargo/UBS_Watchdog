"use client";
import { useState, useEffect } from "react";
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
import { useClientsWithAccountsSimple } from "@/hooks/useClientsWithAccountsSimple";
import { useClientAccounts } from "@/hooks/useClientAccounts";
import { useClientAccountIdentifiers } from "@/hooks/useClientAccountIdentifiers";


interface Props {
  isOpen: boolean;
  onClose: () => void;
  onCreated?: (tx: TransactionResponseDto) => void;
}

export function CreateTransactionDialog({ isOpen, onClose, onCreated }: Props) {

  const [selectedClientId, setSelectedClientId] = useState<string>("");
  const [selectedAccountId, setSelectedAccountId] = useState<string>("");
  const [selectedIdentifierId, setSelectedIdentifierId] = useState<string>("");
  const { clients, loading: loadingClients } = useClientsWithAccountsSimple();
  const { accounts, loading: loadingAccounts } = useClientAccounts(selectedClientId);
  const { identifiers, loading: loadingIdentifiers } = useClientAccountIdentifiers(selectedAccountId);
  const [type, setType] = useState<TransactionTypeApi>(0);
  const [transferMethod, setTransferMethod] = useState<TransferMethodApi | null>(null);
  const [amount, setAmount] = useState<string>("");
  const [currency, setCurrency] = useState("USD");
  const [cpName, setCpName] = useState("");
  const [cpBank, setCpBank] = useState("");
  const [cpBranch, setCpBranch] = useState("");
  const [cpAccount, setCpAccount] = useState("");
  const [cpIdentifierType, setCpIdentifierType] = useState<string | undefined>("none");
  const [cpIdentifier, setCpIdentifier] = useState("");
  const [cpCountryCode, setCpCountryCode] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setSelectedClientId("");
      setSelectedAccountId("");
      setSelectedIdentifierId("");
      setType(0);
      setTransferMethod(null);
      setAmount("");
      setCurrency("USD");
      setCpName("");
      setCpBank("");
      setCpBranch("");
      setCpAccount("");
      setCpIdentifierType("none");
      setCpIdentifier("");
      setCpCountryCode("");
      setError(null);
      setLoading(false);
    }
  }, [isOpen]);

  const handleSubmit = async () => {
    setLoading(true);
    setError(null);
    try {
      if (!selectedAccountId) throw new Error("AccountId is required");
      if (Number(amount) <= 0) throw new Error("Amount must be greater than 0");
      if (!currency) throw new Error("CurrencyCode is required");

      const payload = {
        accountId: selectedAccountId,
        type,
        amount: Number(amount),
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
    } catch (err: any) {
      console.error(err);
      const errorMessage = (err as any).response?.data?.detail 
        || (err as any).response?.data?.title 
        || (err as any).message 
        || "Error creating transaction";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const getIdentifierTypeName = (type: number) => {
    switch (type) {
      case 0: return "CPF";
      case 1: return "CNPJ";
      case 2: return "TAX_ID";
      case 3: return "PASSPORT";
      case 4: return "LEI";
      case 5: return "PIX_EMAIL";
      case 6: return "PIX_PHONE";
      case 7: return "PIX_RANDOM";
      case 8: return "IBAN";
      default: return "OTHER";
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-3xl">
        <DialogHeader>
          <DialogTitle>Create Transaction</DialogTitle>
        </DialogHeader>

        <div className="flex gap-4 py-4">
          <div className="w-full">
            <label className="text-xs font-medium text-slate-500">Cliente</label>
            <Select value={selectedClientId} onValueChange={(v) => { setSelectedClientId(v); setSelectedAccountId(""); setSelectedIdentifierId(""); }}>
              <SelectTrigger className="w-full">
                <SelectValue placeholder={loadingClients ? "Carregando..." : "Selecione um cliente"} />
              </SelectTrigger>
              <SelectContent position="popper" className="w-full !max-h-[200px] overflow-y-auto">
                {clients.map((client) => (
                  <SelectItem key={client.id} value={client.id}>{client.name}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="w-full max-w-xs">
            <label className="text-xs font-medium text-slate-500">Conta</label>
            <Select value={selectedAccountId} onValueChange={(v) => { setSelectedAccountId(v); setSelectedIdentifierId(""); }} disabled={!selectedClientId}>
              <SelectTrigger className="w-full" style={{ minWidth: "220px" }}>
                <SelectValue placeholder={!selectedClientId ? "Selecione um cliente" : (loadingAccounts ? "Carregando..." : "Selecione uma conta")} />
              </SelectTrigger>
              <SelectContent className="w-full" style={{ minWidth: "220px", maxWidth: "320px" }}>
                {accounts.map((acc) => (
                  <SelectItem key={acc.id} value={acc.id}>{acc.accountIdentifier} ({acc.currencyCode})</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        <div className="py-2">
          <div className="mb-4">
            <label className="text-xs font-medium text-slate-500">Account Identifier</label>
            <Select value={selectedIdentifierId} onValueChange={setSelectedIdentifierId} disabled={!selectedAccountId}>
              <SelectTrigger>
                <SelectValue placeholder={!selectedAccountId ? "Select an account" : (loadingIdentifiers ? "Loading..." : "Select an identifier")}/>
              </SelectTrigger>
              <SelectContent>
                {identifiers.map((id) => (
                  <SelectItem key={id.id} value={id.identifierValue}>{id.identifierValue} ({getIdentifierTypeName(id.identifierType)})</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {selectedAccountId && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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
                    <SelectItem value="0">PIX</SelectItem>
                    <SelectItem value="1">TED</SelectItem>
                    <SelectItem value="2">WIRE</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div>
                <label className="text-xs font-medium text-slate-500">Amount</label>
                <Input type="number" value={amount} onChange={(e) => setAmount(e.target.value)} />
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
          )}
        </div>

        {error && (
          <div className="text-red-500 text-sm px-4 pb-2">
            {error}
          </div>
        )}

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
