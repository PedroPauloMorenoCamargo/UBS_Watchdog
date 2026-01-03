import axios from "axios";

const AUTH_STORAGE_KEY = "ubs-monitoring-auth";

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
});

api.interceptors.request.use(
  (config) => {
    const authStorage = localStorage.getItem(AUTH_STORAGE_KEY);
    if (authStorage) {
      try {
        const { state } = JSON.parse(authStorage);
        if (state?.token) {
          config.headers.Authorization = `Bearer ${state.token}`;
        }
      } catch (error) {
        console.error("Erro ao recuperar token de autenticação:", error);
      }
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);
