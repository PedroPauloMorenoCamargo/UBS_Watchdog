// React standard imports
import { useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
// UBS images
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

// Icons
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
 * @description Form error structure. Each property, when present,
 * contains the message ready for UI display.
 *
 * @property {string} [email] - Error message for email field (when invalid).
 * @property {string} [password] - Error message for password field (when invalid).
 */
type FieldErrors = {
  email?: string;
  password?: string;
};

/**
 * @function validate
 * @description Validates the login form fields.
 * Rules:
 * - `email`: required (after trim) and must be a valid email.
 * - `password`: required with a minimum of 4 characters.
 * @param {string} email - Email field value (possibly with spaces).
 * @param {string} password - Password field value.
 * @returns {FieldErrors} Error map by field; empty when valid.
 */
function validate(email: string, password: string): FieldErrors {
  const errors: FieldErrors = {};
  const e = email.trim();
  const p = password;
  if (!e) errors.email = "Email é obrigatório.";
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(e)) errors.email = "Email inválido.";
  if (!p) errors.password = "Senha é obrigatória.";
  else if (p.length < 4) errors.password = "Senha deve ter pelo menos 4 caracteres.";
  return errors;
}


/**
 * @function FeatureCard
 * @description Reusable informational card to list features in the institutional panel.
 * @param {{ title: string; description: string; icon: React.ReactNode }} props
 * @param {string} props.title - Short feature title.
 * @param {string} props.description - Brief description.
 * @param {React.ReactNode} props.icon - Feature icon.
 * @returns {JSX.Element} Feature card.
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
 * @description System login page.
 * @returns {JSX.Element} Complete login page layout.
 */
export function LoginPage() {
  const navigate = useNavigate();

  const { login, status, errorMessage, clearError } = useAuthStore();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);

  const [touched, setTouched] = useState<{ email: boolean; password: boolean }>({
    email: false,
    password: false,
  });

  const emailRef = useRef<HTMLInputElement | null>(null);
  const passwordRef = useRef<HTMLInputElement | null>(null);

  const errors = useMemo(() => validate(email, password), [email, password]);

  const isSubmitting = status === "loading";
  const canSubmit = !isSubmitting && !errors.email && !errors.password;

  /**
   * @function handleSubmit
   * @description Login form submit handler.
   *
   * Flow:
   * 1) Prevents default browser submit.
   * 2) Clears previous error in store.
   * 3) Revalidates fields at submit time.
   * 4) If there are errors: marks fields as touched and focuses the first invalid one.
   * 5) If valid: triggers `login` and navigates to dashboard on success.
   *
   * @param {React.FormEvent} e - Form submit event.
   * @returns {Promise<void>} Promise resolved at the end of the flow.
   */
  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    clearError();

    const currentErrors = validate(email, password);
    const hasErrors = Boolean(currentErrors.email || currentErrors.password);

    if (hasErrors) {
      setTouched({ email: true, password: true });

      if (currentErrors.email) emailRef.current?.focus();
      else if (currentErrors.password) passwordRef.current?.focus();

      return;
    }

    const ok = await login({
      email: email.trim(),
      password,
    });

    if (ok) navigate("/dashboard");
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
          {/* Left panel (branding / context) */}
          <div className="hidden lg:flex">
            <div className="flex w-full flex-col justify-center rounded-2xl border bg-white/70 p-10 shadow-sm backdrop-blur">
              {/* Top */}
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

              {/* Institutional text */}
              <div className="mt-8 space-y-2">
                <div className="text-sm leading-relaxed text-neutral-600">
                  Sistema que centraliza a gestão de clientes, o registro de transações e a geração de alertas
                  para análise e compliance regulatório.
                </div>
              </div>

              {/* Features */}
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
                {/* Authentication error banner (displayed only when store reports error). */}
                {errorMessage && (
                  <div className="mb-5 flex items-start gap-3 rounded-xl border border-red-200 bg-red-100/60 p-4 shadow-sm backdrop-blur-sm">
                    <div className="mt-1 flex h-8 w-8 items-center justify-center rounded-full bg-red-500/20 text-red-700">
                      {/* Alert icon to visually reinforce the failure. */}
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
                  {/* Email */}
                  <div className="space-y-2">
                    <Label htmlFor="email">Usuário</Label>
                    <Input
                      id="email"
                      ref={emailRef}
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      onBlur={() => setTouched((t) => ({ ...t, email: true }))}
                      placeholder="ex: analyst1"
                      autoComplete="email"
                      inputMode="email"
                      aria-invalid={Boolean(touched.email && errors.email)}
                      className={[
                        "rounded-xl",
                        // Adjust classes to highlight the field when invalid and already "touched".
                        touched.email && errors.email
                          ? "border-red-400 focus-visible:ring-red-400"
                          : "",
                      ].join(" ")}
                      disabled={isSubmitting}
                    />
                    {/* Display error message only after field interaction (touched). */}
                    {touched.email && errors.email && (
                      <p className="text-sm text-red-600">{errors.email}</p>
                    )}
                  </div>

                  {/* Password + toggle */}
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
                          // Adjust classes to highlight the field when invalid and already "touched".
                          touched.password && errors.password
                            ? "border-red-400 focus-visible:ring-red-400"
                            : "",
                        ].join(" ")}
                        disabled={isSubmitting}
                      />

                      {/* Password visibility toggle button. */}
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

                    {/* Display error message only after field interaction (touched). */}
                    {touched.password && errors.password && (
                      <p className="text-sm text-red-600">{errors.password}</p>
                    )}
                  </div>

                  {/* Button */}
                  <Button
                    type="submit"
                    className="w-full rounded-xl bg-red-600 hover:bg-red-700"
                    disabled={!canSubmit}
                  >
                    {isSubmitting ? (
                      <>
                        {/* Loading visual indicator. */}
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
