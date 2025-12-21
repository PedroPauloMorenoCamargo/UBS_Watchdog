import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { LoginRequest } from "@/features/auth/auth.api";
import { login as loginApi } from "@/features/auth/auth.api";

/**
 * @typedef {("idle"|"loading"|"authenticated"|"error")} AuthStatus
 * @description Estados possíveis do fluxo de autenticação, usados para controle de UI
 */
type AuthStatus = "idle" | "loading" | "authenticated" | "error";

/**
 * @typedef {Object} AuthUser
 * @description Modelo de usuário no domínio do frontend.
 * @property {string} username - Identificador principal do usuário (nome).
 * @property {string} [displayName] - Nome amigável para exibição na UI.
 */

type AuthUser = {
  username: string;
  displayName?: string;
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
 * @function getMockCredentials
 * @description Recupera credenciais de mock via variáveis de ambiente. Usado como fallback
 * enquanto o backend não está disponível.
 * @returns {{mockUser: string, mockPass: string}} Credenciais do mock (usuário e senha).
 */
function getMockCredentials() {
  const mockUser = import.meta.env.VITE_MOCK_USER ?? "analyst";
  const mockPass = import.meta.env.VITE_MOCK_PASS ?? "ubs123";
  return { mockUser, mockPass };
}

/**
 * @function toFriendlyAuthError
 * @description Normaliza erros de autenticação para uma mensagem amigável e segura para UI,
 * evitando vazamento de detalhes técnicos (ex.: stack traces, mensagens de rede).
 *
 * @param {unknown} _err - Erro capturado durante tentativa de login.
 * @returns {string} Mensagem pronta para exibição ao usuário.
 */
function toFriendlyAuthError(_: unknown): string {
  return "Verifique usuário e senha e tente novamente.";
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
       * @description Executa o fluxo de login.
       * Fluxo:
       * 1) Marca estado como "loading" e limpa erros anteriores.
       * 2) Tenta autenticar via API (quando backend/mock server existir).
       * 3) Em caso de erro, tenta fallback local via credenciais de mock (env).
       * 4) Se falhar, marca estado como "error" e retorna `false`.
       * @param {LoginRequest} payload - DTO contendo `username` e `password`.
       * @returns {Promise<boolean>} `true` se autenticou com sucesso; caso contrário `false`.
       */
      login: async ({ username, password }) => {
        set({ status: "loading", errorMessage: null });

        try {
          const res = await loginApi({ username, password });

          set({
            status: "authenticated",
            isAuthenticated: true,
            token: res.token ?? "mock-token",
            user: res.user ?? { username },
            errorMessage: null,
          });

          return true;
        } catch (err) {
          const { mockUser, mockPass } = getMockCredentials();

          if (username === mockUser && password === mockPass) {
            set({
              status: "authenticated",
              isAuthenticated: true,
              token: "mock-token",
              user: { username: mockUser, displayName: "Analyst" },
              errorMessage: null,
            });
            return true;
          }

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

      logout: () =>
        set({
          status: "idle",
          isAuthenticated: false,
          token: null,
          user: null,
          errorMessage: null,
        }),
    }),
    {
      name: "ubs-monitoring-auth",
      partialize: (state) => ({
        isAuthenticated: state.isAuthenticated,
        token: state.token,
        user: state.user,
      }),
    }
  )
);
