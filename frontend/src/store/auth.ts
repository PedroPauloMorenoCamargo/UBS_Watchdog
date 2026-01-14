import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { AxiosError } from "axios";
import type { LoginRequest } from "@/features/auth/auth.api";
import { login as loginApi } from "@/features/auth/auth.api";
import { AUTH_STORAGE_KEY } from "@/constants/storage";

/**
 * @typedef {("idle"|"loading"|"authenticated"|"error")} AuthStatus
 * @description Estados possíveis do fluxo de autenticação, usados para controle de UI
 */
type AuthStatus = "idle" | "loading" | "authenticated" | "error";

/**
 * @typedef {Object} AuthUser
 * @description 
 * @property {string} id 
 * @property {string} email 
 * @property {string} fullName 
 */
type AuthUser = {
  id: string;
  email: string;
  fullName: string;
};


/**
 * @typedef {Object} AuthState
 * @description Objeto de store de autenticação (estado + ações). Centraliza sessão, estado de UX e ações para login/logout.
 * @property {AuthStatus} status - Estado atual do fluxo de autenticação.
 * @property {boolean} isAuthenticated - Indica se existe sessão autenticada (persistido).
 * @property {string|null} token - Token da sessão (persistido).
 * @property {AuthUser|null} user - Usuário autenticado (persistido).
 * @property {string|null} errorMessage - Mensagem amigável para UI (não persistida).
 * @property {(payload: LoginRequest) => Promise<boolean>} login - Executa login e retorna `true/false`.
 * @property {() => void} logout - Limpa sessão e reseta estado para "idle".
 * @property {() => void} clearError - Limpa apenas a mensagem de erro atual.
 */
type AuthState = {
  status: AuthStatus;
  isAuthenticated: boolean;
  token: string | null;
  user: AuthUser | null;
  errorMessage: string | null;

  login: (payload: LoginRequest) => Promise<boolean>;
  logout: () => void;
  clearError: () => void;
};

/**
 * @function toFriendlyAuthError
 * @description Normaliza erros de autenticação para uma mensagem amigável e segura para UI,
 * evitando vazamento de detalhes técnicos (ex.: stack traces, mensagens de rede).
 *
 * @param {unknown} err - Erro capturado durante tentativa de login.
 * @returns {string} Mensagem pronta para exibição ao usuário.
 */
function toFriendlyAuthError(err: unknown): string {
  if (err && typeof err === 'object' && 'response' in err) {
    const axiosError = err as AxiosError;
    if (axiosError.response?.status === 401) {
      return "Email ou senha incorretos.";
    }
  }
  return "Erro ao conectar com o servidor. Verifique sua conexão.";
}

/**
 * @function useAuthStore
 * @description Store global de autenticação (Zustand) com persistência via `persist`.
 *
 * Responsabilidades:
 * - Executar login/logout
 * - Controlar estado de UX (status/erro)
 * - Persistir apenas dados de sessão necessários (token/usuário/isAuthenticated)
 * @returns {AuthState} Estado e ações de autenticação.
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      status: "idle",
      isAuthenticated: false,
      token: null,
      user: null,
      errorMessage: null,
      
      /**
       * @function clearError
       * @description Limpa a mensagem de erro atual sem alterar o restante do estado.
       *
       * @returns {void}
       */
      clearError: () => set({ errorMessage: null }),

      /**
       * @async
       * @function login
       * @description Executa o fluxo de login contra o backend real.
       * Fluxo:
       * 1) Marca estado como "loading" e limpa erros anteriores.
       * 2) Tenta autenticar via API do backend.
       * 3) Se sucesso, armazena token e dados do analista.
       * 4) Se falhar, marca estado como "error" e retorna `false`.
       * @param {LoginRequest} payload - DTO contendo `email` e `password`.
       * @returns {Promise<boolean>} `true` se autenticou com sucesso; caso contrário `false`.
       */
      login: async ({ email, password }) => {
        set({ status: "loading", errorMessage: null });

        try {
          const res = await loginApi({ email, password });

          set({
            status: "authenticated",
            isAuthenticated: true,
            token: res.token,
            user: {
              id: res.analyst.id,
              email: res.analyst.corporateEmail,
              fullName: res.analyst.fullName,
            },
            errorMessage: null,
          });

          return true;
        } catch (err) {
          set({
            status: "error",
            isAuthenticated: false,
            token: null,
            user: null,
            errorMessage: toFriendlyAuthError(err),
          });

          return false;
        }
      },

      logout: async () => {
         set({ status: "loading" });
         
         await new Promise((resolve) => setTimeout(resolve, 800)); // apenas didático enquanto ainda nao
         //se comunica com backend
          set({
            status: "idle",
            isAuthenticated: false,
            token: null,
            user: null,
            errorMessage: null,
          });
      }
    }),
    {
      name: AUTH_STORAGE_KEY,
      partialize: (state) => ({
        isAuthenticated: state.isAuthenticated,
        token: state.token,
        user: state.user,
      }),
    }
  )
);
