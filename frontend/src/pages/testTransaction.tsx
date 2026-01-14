"use client";
import { useState } from "react";
import { createTransaction } from "@/services/transaction.service";
import type { CreateTransactionDto } from "@/types/Transactions/transaction";

export default function TestTransactionPage() {
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);

  const handleTest = async () => {
    setLoading(true);
    setError(null);

    const payload: CreateTransactionDto = {
      accountId: "853aa7e1-7ecc-4484-a750-0484d6dcb72d", // accountId válido
      type: 0, // Deposit
      amount: 100,
      currencyCode: "USD",
      occurredAtUtc: new Date().toISOString(),
      cpName: "John Doe",
      cpBank: "Bank of America",
      cpBranch: "001",
      cpAccount: "123456-7",
      cpIdentifierType: 0, // CPF
      cpIdentifier: "123.456.789-00",
      cpCountryCode: "BR",
    };

    try {
      const tx = await createTransaction(payload);
      setResult(tx);
      console.log("Transaction criada:", tx);
    } catch (err: any) {
      console.error(err);
      setError(err.response?.data?.title || err.message || "Erro desconhecido");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-4">
      <h1 className="text-xl font-bold">Teste criação de transaction</h1>
      <button
        onClick={handleTest}
        disabled={loading}
        className="mt-4 p-2 bg-blue-600 text-white rounded"
      >
        {loading ? "Criando..." : "Criar Transaction"}
      </button>

      {result && (
        <pre className="mt-4 p-2 bg-slate-100 rounded">{JSON.stringify(result, null, 2)}</pre>
      )}

      {error && (
        <p className="mt-4 text-red-600">Erro: {error}</p>
      )}
    </div>
  );
}
