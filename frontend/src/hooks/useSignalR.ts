import { useEffect, useRef } from "react";
import { signalRService } from "@/services/signalr.service";

export function useSignalR(accessToken: string | null) {
  const isConnecting = useRef(false);

  useEffect(() => {
    if (!accessToken || isConnecting.current) return;

    isConnecting.current = true;

    signalRService
      .connect(accessToken)
      .catch((error) => {
        console.error("Failed to connect to SignalR:", error);
      })
      .finally(() => {
        isConnecting.current = false;
      });

    return () => {
      signalRService.disconnect();
    };
  }, [accessToken]);

  return signalRService;
}
