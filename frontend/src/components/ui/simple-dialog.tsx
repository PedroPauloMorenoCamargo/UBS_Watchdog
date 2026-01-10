// src/components/ui/simple-dialog.tsx
import { X } from "lucide-react";
import { useEffect } from "react";

interface SimpleDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  children: React.ReactNode;
  title?: string;
  description?: string;
}

export default function SimpleDialog({
  open,
  onOpenChange,
  children,
  title,
  description,
}: SimpleDialogProps) {
  // Fechar com tecla ESC
  useEffect(() => {
    const handleEsc = (e: KeyboardEvent) => {
      if (e.key === "Escape") onOpenChange(false);
    };
    
    if (open) {
      document.addEventListener("keydown", handleEsc);
      document.body.style.overflow = "hidden"; // Bloqueia scroll do body
    }
    
    return () => {
      document.removeEventListener("keydown", handleEsc);
      document.body.style.overflow = "unset"; // Restaura scroll
    };
  }, [open, onOpenChange]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      {/* Overlay - fundo escuro */}
      <div
        className="fixed inset-0 bg-black/50 backdrop-blur-sm"
        onClick={() => onOpenChange(false)}
        aria-hidden="true"
      />

      {/* Dialog - conteúdo */}
      <div className="relative z-50 w-full max-w-lg max-h-[90vh] overflow-y-auto rounded-lg bg-white shadow-xl">
        <div className="p-6">
          {/* Botão fechar */}
          <button
            onClick={() => onOpenChange(false)}
            className="absolute right-4 top-4 rounded-sm opacity-70 transition-opacity hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-slate-400 focus:ring-offset-2"
            aria-label="Fechar"
          >
            <X className="h-4 w-4" />
          </button>

          {/* Header */}
          {(title || description) && (
            <div className="mb-6 pr-8">
              {title && (
                <h2 className="text-xl font-semibold text-slate-900">
                  {title}
                </h2>
              )}
              {description && (
                <p className="mt-2 text-sm text-slate-600">
                  {description}
                </p>
              )}
            </div>
          )}

          {/* Conteúdo */}
          <div>{children}</div>
        </div>
      </div>
    </div>
  );
}


