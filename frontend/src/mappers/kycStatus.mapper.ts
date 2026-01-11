import type { KYC } from "@/types/kycstatus";

export function mapKycStatus(
  status: 0 | 1 | 2 | 3
): KYC {
  switch (status) {
    case 0:
      return "Pending";
    case 1:
      return "Verified";
    case 2:
      return "Expired";
    case 3:
      return "Rejected";
  }
}
