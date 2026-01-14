import type { Status } from "@/types/status";

export type StatusApi = 0 | 1 | 2 ;

export function mapStatus(
  status: 0 | 1 | 2 
): Status {
  switch (status) {
    case 0:
      return "New";
    case 1:
      return "Under Review";
    case 2:
      return "Resolved";
  }
}