export type KYC = "Expired" | "Pending" | "Verified" | "Rejected";
export type KycFilter = KYC | "all";

export type KycStatusApi = 0 | 1 | 2 | 3;