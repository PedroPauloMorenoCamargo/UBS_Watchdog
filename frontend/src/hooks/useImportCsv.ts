// src/hooks/useImportCsv.ts
// import { useState } from "react";
// import { importClientsCsv } from "@/services/clients.service";
// import type { ImportCsvResponseDto } from "@/types/clients/client";

// interface UseImportCsvResult {
//   importCsv: (file: File) => Promise<{
//     success: boolean;
//     data?: ImportCsvResponseDto;
//     error?: string;
//   }>;
//   isImporting: boolean;
//   error: string | null;
// }

// export function useImportCsv(): UseImportCsvResult {
//   const [isImporting, setIsImporting] = useState(false);
//   const [error, setError] = useState<string | null>(null);

//   const importCsv = async (file: File) => {
//     setIsImporting(true);
//     setError(null);

//     try {
//       const result = await importClientsCsv(file);
//       return { success: true, data: result };
//     } catch (err: any) {
//       const errorMessage =
//         err.response?.data?.message ??
//         err.message ??
//         "Erro ao importar CSV";

//       setError(errorMessage);
//       return { success: false, error: errorMessage };
//     } finally {
//       setIsImporting(false);
//     }
//   };

//   return {
//     importCsv,
//     isImporting,
//     error,
//   };
// }


import { useState } from "react";
import { importClientsCsv } from "@/services/clients.service";

export function useImportClientsCsv(onSuccess?: () => void) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit(file: File) {
    try {
      setLoading(true);
      setError(null);
      await importClientsCsv(file);
      onSuccess?.();
    } catch (e) {
      console.error(e);
      setError("Failed to import CSV");
    } finally {
      setLoading(false);
    }
  }

  return { submit, loading, error };
}
