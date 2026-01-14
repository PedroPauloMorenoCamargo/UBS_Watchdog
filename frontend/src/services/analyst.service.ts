import { api } from "@/lib/api";
import type { AnalystProfileResponse } from "@/types/analyst";

export function fetchAllAnalysts() {
  return api.get<AnalystProfileResponse[]>("api/analysts").then((res) => res.data);
}
