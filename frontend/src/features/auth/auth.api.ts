import { api } from "@/lib/api";

/**
 * DTO enviado do endpoint de login.
 */
export type LoginRequest = {
  username: string;
  password: string;
};

/**
 * DTO de resposta do login (mock).
 *
 * Nota:
 * - Campos opcionais são intencionais aqui para suportar cenários de mock. Mudar no Futuro para interagir com server.
 * - Em caso real de produção, `token` e `user` devem ser obrigatórios
 */
export type LoginResponse = {
  token?: string;
  user?: {
    username: string;

    /**
     * Nome “amigável” para exibição na UI.
     * Opcional porque o mock pode não fornecer esse dado.
     */
    displayName?: string;
  };
};

/**
 * Autentica via endpoint de login (mock).
 *
 * Endpoint (mock): POST /login
 *
 * Retorno:
 * - Retorna apenas o `data` da resposta.
 *
 * Erros:
 * - Erros de rede/HTTP serão propagados pelo client (`api`) e devem ser tratados no chamador (ex.: hook, action, componente).
 */
export async function login(request: LoginRequest): Promise<LoginResponse> {
  const { data } = await api.post<LoginResponse>("/login", request);
  return data;
}
