import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "@/store/auth";

import { Loader2, Bell, User } from "lucide-react";

export function Header() {
  const [openProfile, setOpenProfile] = useState(false);
  const [openNotifications, setOpenNotifications] = useState(false);

  
  const navigate = useNavigate();
  const {logout, status}  = useAuthStore();

  const isLoggingOut = status === "loading";

  async function handleLogout() {
   await logout(); // limpa Zustand + persist
   navigate("/", { replace: true });
   setOpenProfile(false);
   setOpenNotifications(false);
  }

  const profileRef = useRef<HTMLDivElement>(null);
  const notificationsRef = useRef<HTMLDivElement>(null);

  const user = useAuthStore((s) => s.user);
  const adminName = user?.fullName ?? "Usuário";

  
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
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  return (
    <header className="h-16 bg-white border-b flex items-center justify-between px-6 relative z-50">
      {/* Nome */}
      <div className="font-semibold text-slate-700">
        Bem-vindo, {adminName}
      </div>

      {/* Ações */}
      <div className="relative flex items-center gap-4">

        {/* Perfil */}
        <div ref={profileRef} className="relative">
          <button
            onClick={() => {
              setOpenProfile(!openProfile);
              setOpenNotifications(false);
            }}
            className="w-9 h-9 rounded-full bg-slate-300 flex items-center justify-center
             overflow-hidden cursor-pointer hover:bg-slate-200 transition"
          >
            <User size={18} />
          </button>

          {openProfile && (
            <div className="absolute right-0 top-12 w-56 bg-white border rounded-lg shadow-lg p-3">
              <p className="font-semibold">{adminName}</p>
              <p className="text-sm text-slate-500 mb-3">{user?.email}</p>

              <div className="border-t pt-2 space-y-2 text-sm">
                <button className="w-full text-left cursor-pointer hover:text-blue-600">
                  Meu perfil
                </button>
                <button className="w-full text-left cursor-pointer hover:text-blue-600">
                  Configurações
                </button>
                <button
                  onClick={handleLogout}
                  disabled={isLoggingOut}
                  className="w-full text-left text-red-600 flex items-center cursor-pointer gap-2 disabled:opacity-70"
                >
                  {isLoggingOut ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Efetuando logout...
                    </>
                  ) : (
                    "Sair"
                  )}
                </button>

              </div>
            </div>
          )}
        </div>

        {/* Notificações */}
        <div ref={notificationsRef} className="relative">
          <button
            onClick={() => {
              setOpenNotifications(!openNotifications);
              setOpenProfile(false);
            }}
            className="relative p-2 rounded-full hover:bg-slate-200 cursor-pointer transition"
          >
            <Bell size={20} />
          </button>

          {openNotifications && (
            <div className="absolute right-0 top-12 w-64 bg-white border rounded-lg shadow-lg p-3">
              <p className="font-semibold mb-2">Notificações</p>
              <ul className="text-sm text-slate-600 space-y-1">
                <li>🔔 Nova transação registrada</li>
                <li>⚠️ Alerta de risco</li>
                <li>📊 Relatório disponível</li>
              </ul>
            </div>
          )}
        </div>

      </div>
    </header>
  );
}
