// import { useState, useEffect } from "react";
// import type { AxiosError } from "axios";
// import type { AccountResponseDto } from "@/types/Transactions/transaction";
// import { api } from "@/lib/api";

// /**
//  * Hook para carregar informações da conta do usuário logado
//  */
// export function useMyAccount() {
//   const [account, setAccount] = useState<AccountResponseDto | null>(null);
//   const [loading, setLoading] = useState(true);
//   const [error, setError] = useState<string | null>(null);

//   const loadAccount = async () => {
//     setLoading(true);
//     setError(null);

//     try {
//       // Se não houver accountId logado, apenas retorna null
//       // Ajuste aqui caso você tenha uma forma de obter accountId
//       const accountId = ""; // deixar vazio se não disponível

//       if (!accountId) {
//         setAccount(null);
//         return;
//       }

//       const res = await api.get<AccountResponseDto>(
//         `/api/accounts/${accountId}/identifiers`
//       );

//       setAccount(res.data);
//     } catch (err: unknown) {
//       const axiosErr = err as AxiosError;
//       console.error("Error loading account:", axiosErr);
//       setError(
//         axiosErr.response?.data?.message || "Failed to load account information."
//       );
//       setAccount(null);
//     } finally {
//       setLoading(false);
//     }
//   };

//   useEffect(() => {
//     loadAccount();
//   }, []);

//   return { account, loading, error, refetch: loadAccount };
// }
