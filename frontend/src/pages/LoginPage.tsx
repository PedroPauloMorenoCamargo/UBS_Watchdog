// Imports Padrões do React
import { useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
// Imagens da UBS
import UbsLogo from "@/assets/svg/ubs_logo.svg";
import loginBg from "@/assets/png/ubs.jpg";

import { useAuthStore } from "@/store/auth";

// Shadcn/ui
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

// Icones
import {
  Eye,
  EyeOff,
  Loader2,
  ShieldAlert,
  Users,
  ArrowLeftRight,
  ShieldCheck,
  FileBarChart,
} from "lucide-react";

/**
 * @typedef {Object} FieldErrors
 * @description Estrutura de erros do formulário. Cada propriedade, quando presente,
 * contém a mensagem pronta para exibição na UI.
 *
 * @property {string} [username] - Mensagem de erro do campo usuário (quando inválido).
 * @property {string} [password] - Mensagem de erro do campo senha (quando inválido).
 */
type FieldErrors = {
  username?: string;
  password?: string;
};

/**
 * @function validate
 * @description Valida os campos do formulário de login.
 * Regras:
 * - `username`: obrigatório (após trim).
 * - `password`: obrigatório e com mínimo de 4 caracteres.
 * @param {string} username - Valor do campo de usuário (possivelmente com espaços).
 * @param {string} password - Valor do campo de senha.
 * @returns {FieldErrors} Mapa de erros por campo; vazio quando válido.
 */
function validate(username: string, password: string): FieldErrors {
  const errors: FieldErrors = {};
  const u = username.trim();
  const p = password;
  if (!u) errors.username = "Usuário é obrigatório.";
  if (!p) errors.password = "Senha é obrigatória.";
  else if (p.length < 4) errors.password = "Senha deve ter pelo menos 4 caracteres.";
  return errors;
}


/**
 * @function FeatureCard
 * @description  Cartão informativo reutilizável para listar funcionalidades no painel institucional.
 * @param {{ title: string; description: string; icon: React.ReactNode }} props
 * @param {string} props.title - Título curto da funcionalidade.
 * @param {string} props.description - Descrição breve.
 * @param {React.ReactNode} props.icon - Ícone da funcionalidade.
 * @returns {JSX.Element} Card de feature.
 */
function FeatureCard(props: {
  title: string;
  description: string;

  icon: React.ReactNode;
}) {
  return (
    <div className="rounded-xl border bg-white/70 p-4 shadow-sm backdrop-blur">
      <div className="flex items-start gap-3">
        <div className="mt-0.5 rounded-lg bg-red-600/10 p-2 text-red-700">
          {props.icon}
        </div>
        <div className="min-w-0">
          <div className="text-sm font-semibold text-neutral-900">
            {props.title}
          </div>
          <div className="mt-1 text-xs leading-relaxed text-neutral-600">
            {props.description}
          </div>
        </div>
      </div>
    </div>
  );
}

/**
 * @function LoginPage
 * @description Página de login do sistema.
 * @returns {JSX.Element} Layout completo da página de login.
 */
export function LoginPage() {
  const navigate = useNavigate();

  const { login, status, errorMessage, clearError } = useAuthStore();

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);

  const [touched, setTouched] = useState<{ username: boolean; password: boolean }>({
    username: false,
    password: false,
  });

  const usernameRef = useRef<HTMLInputElement | null>(null);
  const passwordRef = useRef<HTMLInputElement | null>(null);

  const errors = useMemo(() => validate(username, password), [username, password]);

  const isSubmitting = status === "loading";
  const canSubmit = !isSubmitting && !errors.username && !errors.password;

  /**
   * @function handleSubmit
   * @description Handler de submit do formulário de login.
   *
   * Fluxo:
   * 1) Impede submit padrão do browser.
   * 2) Limpa erro anterior no store.
   * 3) Revalida campos no momento do submit.
   * 4) Se houver erros: marca campos como touched e foca o primeiro inválido.
   * 5) Se válido: dispara `login` e navega para a rota de clientes ao sucesso.
   *
   * @param {React.FormEvent} e - Evento de submit do formulário.
   * @returns {Promise<void>} Promise resolvida ao final do fluxo.
   */
  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    clearError();

    const currentErrors = validate(username, password);
    const hasErrors = Boolean(currentErrors.username || currentErrors.password);

    if (hasErrors) {
      setTouched({ username: true, password: true });

      if (currentErrors.username) usernameRef.current?.focus();
      else if (currentErrors.password) passwordRef.current?.focus();

      return;
    }

    const ok = await login({
      username: username.trim(),
      password,
    });

    if (ok) navigate("/clients");
  }

  return (
    <div
      className="min-h-screen bg-cover bg-center bg-no-repeat"
      style={{
        backgroundImage: `url(${loginBg})`,
      }}
    >
      <div className="mx-auto flex min-h-screen w-full max-w-6xl items-center justify-center p-4">
        <div className="grid w-full grid-cols-1 gap-6 lg:grid-cols-2">
          {/* Painel esquerda (branding / contexto) */}
          <div className="hidden lg:flex">
            <div className="flex w-full flex-col justify-center rounded-2xl border bg-white/70 p-10 shadow-sm backdrop-blur">
              {/* Topo */}
              <div className="flex items-center gap-3">
                <div className="flex h-15 w-15 items-center justify-center">
                  <img
                    src={UbsLogo}
                    alt="UBS Watchdog"
                    className="h-15 w-15"
                  />
                </div>

                <div>
                  <div className="text-sm font-medium text-neutral-700">
                    UBS Watchdog
                  </div>
                  <div className="text-xl font-semibold tracking-tight text-neutral-900">
                    Monitoramento de Transações
                  </div>
                </div>
              </div>

              {/* Texto institucional */}
              <div className="mt-8 space-y-2">
                <div className="text-sm leading-relaxed text-neutral-600">
                  Sistema que centraliza a gestão de clientes, o registro de transações e a geração de alertas
                  para análise e compliance regulatório.
                </div>
              </div>

              {/* Funcionalidades */}
              <div className="mt-8">
                <div className="text-sm font-semibold text-neutral-900">
                  Funcionalidades
                </div>

                <div className="mt-4 grid auto-rows-fr grid-cols-2 gap-3">
                  <FeatureCard
                    title="Gestão de Clientes"
                    description="Cadastro e consulta com perfil de risco e status de KYC."
                    icon={<Users className="h-4 w-4" />}
                  />
                  <FeatureCard
                    title="Registro de Transações"
                    description="Depósitos, saques e transferências com histórico e filtros."
                    icon={<ArrowLeftRight className="h-4 w-4" />}
                  />
                  <FeatureCard
                    title="Regras & Alertas"
                    description="Avaliação automática e fluxo de revisão de alertas."
                    icon={<ShieldCheck className="h-4 w-4" />}
                  />
                  <FeatureCard
                    title="Relatórios Analíticos"
                    description="Indicadores e visualizações para acompanhamento."
                    icon={<FileBarChart className="h-4 w-4" />}
                  />
                </div>
              </div>
            </div>
          </div>

          {/* Form */}
          <div className="flex items-center justify-center">
            <Card className="w-full max-w-md rounded-2xl border bg-white/80 shadow-sm backdrop-blur">
              <CardHeader className="space-y-2">
                <CardTitle className="text-2xl">Entrar</CardTitle>
                <CardDescription>
                  Use suas credenciais para acessar o sistema.
                </CardDescription>
              </CardHeader>

              <CardContent>
                {/* Banner de erro de autenticação (exibido apenas quando o store reporta erro). */}
                {errorMessage && (
                  <div className="mb-5 flex items-start gap-3 rounded-xl border border-red-200 bg-red-100/60 p-4 shadow-sm backdrop-blur-sm">
                    <div className="mt-1 flex h-8 w-8 items-center justify-center rounded-full bg-red-500/20 text-red-700">
                      {/* Ícone do alerta para reforçar visualmente a falha. */}
                      <ShieldAlert className="h- w-6" />
                    </div>
                    <div className="text-sm">
                      <div className="font-semibold text-red-800">
                        Falha na autenticação.
                      </div>
                      <div className="mt-0.5 text-red-700">{errorMessage}</div>
                    </div>
                  </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-4">
                  {/* Usuário */}
                  <div className="space-y-2">
                    <Label htmlFor="username">Usuário</Label>
                    <Input
                      id="username"
                      ref={usernameRef}
                      value={username}
                      onChange={(e) => setUsername(e.target.value)}
                      onBlur={() => setTouched((t) => ({ ...t, username: true }))}
                      placeholder="ex: analyst"
                      autoComplete="username"
                      inputMode="text"
                      aria-invalid={Boolean(touched.username && errors.username)}
                      className={[
                        "rounded-xl",
                        // Ajusta classes para destacar o campo quando inválido e já "tocado".
                        touched.username && errors.username
                          ? "border-red-400 focus-visible:ring-red-400"
                          : "",
                      ].join(" ")}
                      disabled={isSubmitting}
                    />
                    {/* Exibe mensagem de erro apenas após interação com o campo (touched). */}
                    {touched.username && errors.username && (
                      <p className="text-sm text-red-600">{errors.username}</p>
                    )}
                  </div>

                  {/* Senha + toggle */}
                  <div className="space-y-2">
                    <Label htmlFor="password">Senha</Label>
                    <div className="relative">
                      <Input
                        id="password"
                        ref={passwordRef}
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        onBlur={() => setTouched((t) => ({ ...t, password: true }))}
                        placeholder="••••••••"
                        type={showPassword ? "text" : "password"}
                        autoComplete="current-password"
                        aria-invalid={Boolean(touched.password && errors.password)}
                        className={[
                          "rounded-xl pr-11",
                          // Ajusta classes para destacar o campo quando inválido e já "tocado".
                          touched.password && errors.password
                            ? "border-red-400 focus-visible:ring-red-400"
                            : "",
                        ].join(" ")}
                        disabled={isSubmitting}
                      />

                      {/* Botão de alternância de visibilidade da senha. */}
                      <button
                        type="button"
                        onClick={() => setShowPassword((v) => !v)}
                        className="absolute right-2 top-1/2 -translate-y-1/2 rounded-lg p-2 text-neutral-600 hover:bg-neutral-100"
                        aria-label={showPassword ? "Ocultar senha" : "Mostrar senha"}
                        disabled={isSubmitting}
                      >
                        {showPassword ? (
                          <EyeOff className="h-4 w-4" />
                        ) : (
                          <Eye className="h-4 w-4" />
                        )}
                      </button>
                    </div>

                    {/* Exibe mensagem de erro apenas após interação com o campo (touched). */}
                    {touched.password && errors.password && (
                      <p className="text-sm text-red-600">{errors.password}</p>
                    )}
                  </div>

                  {/* Botão */}
                  <Button
                    type="submit"
                    className="w-full rounded-xl bg-red-600 hover:bg-red-700"
                    disabled={!canSubmit}
                  >
                    {isSubmitting ? (
                      <>
                        {/* Indicador visual de carregamento. */}
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Entrando...
                      </>
                    ) : (
                      "Login"
                    )}
                  </Button>
                </form>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
}
