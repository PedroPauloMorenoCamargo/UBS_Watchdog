import * as signalR from "@microsoft/signalr";

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private listeners: Map<string, Set<(data: any) => void>> = new Map();

  async connect(accessToken: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      console.log("SignalR already connected");
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_API_BASE_URL}/hubs/cases`, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.onclose((error) => {
      console.error("SignalR connection closed:", error);
    });

    this.connection.onreconnecting((error) => {
      console.warn("SignalR reconnecting:", error);
    });

    this.connection.onreconnected((connectionId) => {
      console.log("SignalR reconnected:", connectionId);
    });

    try {
      await this.connection.start();
      console.log("SignalR connected successfully");
      this.registerListeners();
    } catch (error) {
      console.error("SignalR connection error:", error);
      throw error;
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      this.listeners.clear();
      console.log("SignalR disconnected");
    }
  }

  on(event: string, callback: (data: any) => void): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);

    // If already connected, register the listener immediately
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      this.connection.on(event, callback);
    }
  }

  off(event: string, callback: (data: any) => void): void {
    const eventListeners = this.listeners.get(event);
    if (eventListeners) {
      eventListeners.delete(callback);
      if (eventListeners.size === 0) {
        this.listeners.delete(event);
      }
    }

    if (this.connection) {
      this.connection.off(event, callback);
    }
  }

  private registerListeners(): void {
    if (!this.connection) return;

    this.listeners.forEach((callbacks, event) => {
      callbacks.forEach((callback) => {
        this.connection!.on(event, callback);
      });
    });
  }

  getConnectionState(): signalR.HubConnectionState | null {
    return this.connection?.state ?? null;
  }
}

export const signalRService = new SignalRService();
