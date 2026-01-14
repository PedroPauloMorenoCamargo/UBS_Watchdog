import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "@/store/auth";
import { api } from "@/lib/api";
import { Loader2, Bell, User, Upload, Trash2 } from "lucide-react";

type MeResponse = {
  id: string;
  corporateEmail?: string | null;
  fullName?: string | null;
  phoneNumber?: string | null;
  profilePictureBase64?: string | null;
  createdAtUtc?: string | null;
};

function guessImageMimeFromBase64(b64: string) {
  if (b64.startsWith("iVBORw0KGgo")) return "image/png";
  if (b64.startsWith("/9j/")) return "image/jpeg";
  return "image/png";
}

function formatUtc(utc?: string | null) {
  if (!utc) return "—";
  const d = new Date(utc);
  if (Number.isNaN(d.getTime())) return "—";
  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(d);
}

export function Header() {
  const [openProfile, setOpenProfile] = useState(false);
  const [openNotifications, setOpenNotifications] = useState(false);

  const [me, setMe] = useState<MeResponse | null>(null);
  const [isRefreshingMe, setIsRefreshingMe] = useState(false);

  const [isUploadingPhoto, setIsUploadingPhoto] = useState(false);
  const [photoError, setPhotoError] = useState<string | null>(null);

  const navigate = useNavigate();
  const { logout, status } = useAuthStore();
  const isLoggingOut = status === "loading";

  const userFromStore = useAuthStore((s) => s.user) as any;

  const adminName =
    me?.fullName ?? userFromStore?.fullName ?? "Usuário";

  const email =
    me?.corporateEmail ??
    userFromStore?.corporateEmail ??
    userFromStore?.email ??
    "—";

  const phone =
    me?.phoneNumber ??
    userFromStore?.phoneNumber ??
    "—";

  const createdAt = formatUtc(me?.createdAtUtc);

  const avatarSrc = useMemo(() => {
    const b64 =
      me?.profilePictureBase64 ??
      userFromStore?.profilePictureBase64 ??
      null;

    if (!b64) return null;
    const mime = guessImageMimeFromBase64(b64);
    return `data:${mime};base64,${b64}`;
  }, [me?.profilePictureBase64, userFromStore?.profilePictureBase64]);

  async function handleLogout() {
    await logout();
    navigate("/", { replace: true });
    setOpenProfile(false);
    setOpenNotifications(false);
  }

  const profileRef = useRef<HTMLDivElement>(null);
  const notificationsRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      const target = event.target as Node;

      if (
        profileRef.current &&
        !profileRef.current.contains(target) &&
        notificationsRef.current &&
        !notificationsRef.current.contains(target)
      ) {
        setOpenProfile(false);
        setOpenNotifications(false);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);
    return () =>
      document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  async function refreshMe() {
    setIsRefreshingMe(true);
    try {
      const { data } = await api.get<MeResponse>("/api/auth/me");
      setMe(data);
    } finally {
      setIsRefreshingMe(false);
    }
  }

  async function patchProfilePicture(profilePictureBase64: string | null) {
    await api.patch("/api/analysts/me/profile-picture", {
      profilePictureBase64,
    });
  }

  function readFileAsDataUrl(file: File) {
    return new Promise<string>((resolve, reject) => {
      const reader = new FileReader();
      reader.onerror = () => reject(new Error("Falha ao ler arquivo"));
      reader.onload = () => resolve(String(reader.result));
      reader.readAsDataURL(file);
    });
  }

  async function handlePhotoChange(file: File) {
    setPhotoError(null);

    if (!["image/png", "image/jpeg"].includes(file.type)) {
      setPhotoError("Envie uma imagem PNG ou JPG.");
      return;
    }

    if (file.size > 2 * 1024 * 1024) {
      setPhotoError("Imagem deve ter no máximo 2MB.");
      return;
    }

    setIsUploadingPhoto(true);
    try {
      const dataUrl = await readFileAsDataUrl(file);
      const base64 = dataUrl.split(",")[1];

      if (!base64) {
        throw new Error("Base64 inválido.");
      }

      await patchProfilePicture(base64);

      setMe((prev) => ({
        ...(prev ?? ({} as MeResponse)),
        profilePictureBase64: base64,
      }));
    } catch (e: any) {
      setPhotoError(e?.message ?? "Erro ao atualizar foto.");
    } finally {
      setIsUploadingPhoto(false);
    }
  }

  async function handleClearPhoto() {
    setPhotoError(null);
    setIsUploadingPhoto(true);
    try {
      await patchProfilePicture(null);
      setMe((prev) => ({
        ...(prev ?? ({} as MeResponse)),
        profilePictureBase64: null,
      }));
    } catch (e: any) {
      setPhotoError(e?.message ?? "Erro ao remover foto.");
    } finally {
      setIsUploadingPhoto(false);
    }
  }

  return (
    <header className="h-16 bg-background border-b border-border flex items-center justify-between px-6 relative z-50">
      <div className="font-semibold text-foreground">
        Bem-vindo, {adminName}
      </div>

      <div className="relative flex items-center gap-4">
        {/* Notificações */}
        <div ref={notificationsRef} className="relative">
          <button
            onClick={() => {
              setOpenNotifications(!openNotifications);
              setOpenProfile(false);
            }}
            className="p-2 rounded-full hover:bg-muted text-muted-foreground hover:text-foreground transition-colors"
          >
            <Bell size={20} />
          </button>
        </div>

        {/* Perfil */}
        <div ref={profileRef} className="relative">
          <button
            onClick={() => {
              const next = !openProfile;
              setOpenProfile(next);
              setOpenNotifications(false);
              if (next) refreshMe();
            }}
            className="w-9 h-9 rounded-full bg-muted overflow-hidden flex items-center justify-center hover:bg-muted/80 ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
          >
            {avatarSrc ? (
              <img
                src={avatarSrc}
                alt="Avatar"
                className="w-full h-full object-cover"
              />
            ) : (
              <User size={18} className="text-muted-foreground" />
            )}
          </button>

          {openProfile && (
            <div className="absolute right-0 top-12 w-80 bg-popover border border-border rounded-lg shadow-lg p-4 animate-in fade-in zoom-in-95 duration-200">
              <div className="flex gap-3">
                <div className="w-12 h-12 rounded-full bg-muted overflow-hidden flex items-center justify-center">
                  {avatarSrc ? (
                    <img src={avatarSrc} className="w-full h-full object-cover" />
                  ) : (
                    <User size={20} className="text-muted-foreground" />
                  )}
                </div>

                <div className="flex-1 min-w-0">
                  <p className="font-semibold truncate text-foreground">{adminName}</p>
                  <p className="text-sm text-muted-foreground truncate">{email}</p>
                   <p className="text-sm text-muted-foreground truncate">{phone}</p>
                  <p className="text-xs text-muted-foreground/70">
                    {isRefreshingMe ? "Carregando..." : `Criado em: ${createdAt}`}
                  </p>
                </div>
              </div>

              <div className="mt-4 border-t border-border pt-3">
                <p className="font-semibold text-sm mb-2 text-foreground">Foto do perfil</p>

                <div className="flex items-center gap-3">
                  <label className="flex items-center gap-2 text-primary cursor-pointer text-sm font-medium hover:text-primary/90">
                    <Upload className="h-4 w-4" />
                    {isUploadingPhoto ? "Enviando..." : "Alterar (PNG/JPG)"}
                    <input
                      type="file"
                      accept="image/png,image/jpeg"
                      className="hidden"
                      disabled={isUploadingPhoto}
                      onChange={(e) => {
                        const file = e.target.files?.[0];
                        e.target.value = "";
                        if (file) handlePhotoChange(file);
                      }}
                    />
                  </label>

                  <button
                    onClick={handleClearPhoto}
                    disabled={isUploadingPhoto}
                    className="flex items-center gap-1 text-sm text-muted-foreground hover:text-destructive transition-colors"
                  >
                    <Trash2 className="h-4 w-4" />
                    Remover
                  </button>
                </div>

                {photoError && (
                  <p className="mt-2 text-xs text-destructive">{photoError}</p>
                )}
              </div>

              <div className="mt-4 border-t border-border pt-3">
                <button
                  onClick={handleLogout}
                  disabled={isLoggingOut}
                  className="text-destructive flex items-center gap-2 text-sm font-medium hover:text-destructive/80 transition-colors"
                >
                  {isLoggingOut ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Saindo...
                    </>
                  ) : (
                    "Sair"
                  )}
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}
