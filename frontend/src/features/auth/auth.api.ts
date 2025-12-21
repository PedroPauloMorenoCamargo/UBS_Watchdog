import { api } from "@/lib/api";

/**
 * @typedef {Object} LoginRequest
 * @description DTO enviado ao endpoint de login.
 * @property {string} username - Identificador do usuário (login).
 * @property {string} password - Senha em texto puro.
 */
export type LoginRequest = {
  username: string;
  password: string;
};

/**
 * @typedef {Object} LoginResponse
 * @description DTO de resposta do endpoint de login (mock).
 * @property {string} [token] - Token de autenticação retornado pelo backend/mock.
 * @property {Object} [user] - Dados básicos do usuário autenticado.
 * @property {string} user.username - Identificador do usuário.
 * @property {string} [user.displayName] - Nome amigável para exibição na UI.
 */
export type LoginResponse = {
  token?: string;
  user?: {
    username: string;
    displayName?: string;
  };
};

/**
 * @async
 * @function login
 * @description Executa autenticação via endpoint de login.
 *
 * Endpoint esperado:
 * - POST /login
 *
 * Comportamento:
 * - Envia as credenciais para o backend/mock.
 * - Retorna apenas o payload (`data`) da resposta.
 *
 * Tratamento de erros:
 * - Erros de rede, timeout ou HTTP (4xx/5xx) **não são tratados aqui**.
 * @param {LoginRequest} request - DTO contendo credenciais de login.
 * @returns {Promise<LoginResponse>} DTO de resposta do login.
 */
export async function login(request: LoginRequest): Promise<LoginResponse> {
  const { data } = await api.post<LoginResponse>("/login", request);
  return data;
}
