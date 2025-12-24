import { useState } from "react";
import { Bell, User } from "lucide-react";

export function Header() {
  const [openProfile, setOpenProfile] = useState(false);
  const [openNotifications, setOpenNotifications] = useState(false);

  // depois você pode trocar por dados reais do auth/context
  const adminName = "Admin UBS";

  return (
    <header className="h-16 bg-white border-b flex items-center justify-between px-6 relative z-50">
      {/* Nome do usuário */}
      <div className="font-semibold text-slate-700">
        Bem-vindo, {adminName}
      </div>

      {/* Ações */}
      <div className="relative flex items-center gap-4">

        {/* Perfil */}
        <button
          onClick={() => {
            setOpenProfile(!openProfile);
            setOpenNotifications(false);
          }}
          className="w-9 h-9 rounded-full bg-slate-300 flex items-center justify-center
           overflow-hidden cursor-pointer hover:bg-slate-200 transition"
        >
          {/* futuramente pode ser uma imagem */}
          <User size={18} />
        </button>

        {/* Dropdown Perfil */}
        {openProfile && (
          <div className="absolute right-0 top-12 w-56 bg-white border rounded-lg shadow-lg p-3">
            <p className="font-semibold">{adminName}</p>
            <p className="text-sm text-slate-500 mb-3">admin@ubs.com</p>

            <div className="border-t pt-2 space-y-2 text-sm">
              <button className="w-full text-left hover:text-blue-600">
                Meu perfil
              </button>
              <button className="w-full text-left hover:text-blue-600">
                Configurações
              </button>
              <button className="w-full text-left text-red-600">
                Sair
              </button>
            </div>
          </div>
        )}

        {/* Sino */}
        <button
          onClick={() => {
            setOpenNotifications(!openNotifications);
            setOpenProfile(false);
          }}
          className="relative p-2 rounded-full hover:bg-slate-100 cursor-pointer hover:bg-slate-200 transition"
        >
          <Bell size={20} />
        </button>

        {/* Dropdown Notificações */}
        {openNotifications && (
          <div className="absolute right-12 top-12 w-64 bg-white border rounded-lg shadow-lg p-3 z-50">
            <p className="font-semibold mb-2">Notificações</p>
            <ul className="text-sm text-slate-600 space-y-1">
              <li>🔔 Nova transação registrada</li>
              <li>⚠️ Alerta de risco</li>
              <li>📊 Relatório disponível</li>
            </ul>
          </div>
        )}

        
      </div>
    </header>
  );
}
