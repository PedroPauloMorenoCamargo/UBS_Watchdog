import { api } from "@/lib/api";

/**
 * @typedef {Object} LoginRequest
 * @description 
 * @property {string} email 
 * @property {string} password 
 */
export type LoginRequest = {
  email: string;
  password: string;
};

/**
 * @typedef {Object} AnalystProfile
 * @description 
 */
export type AnalystProfile = {
  id: string;
  corporateEmail: string;
  fullName: string;
  phoneNumber?: string | null;
  profilePictureBase64?: string | null;
  createdAtUtc: string;
};

/**
 * @typedef {Object} LoginResponse
 * @description 
 * @property {string} token 
 * @property {string} expiresAtUtc 
 * @property {AnalystProfile} analyst 
 */
export type LoginResponse = {
  token: string;
  expiresAtUtc: string;
  analyst: AnalystProfile;
};

/**
 * @async
 * @function login
 * @description - Authentication is performed via the backend login endpoint.
 *
 * Endpoint:
 * - POST /api/auth/login
 *
 * Behavior :
 * - Sends the credentials to the backend.
 * - Returns the payload containing the JWT token and analyst profile.
 *
 * Errors handling:
 * - Network, timeout, or HTTP (4xx/5xx) errors are propagated to the caller.
 * @param {LoginRequest} request 
 * @returns {Promise<LoginResponse>} 
 */
export async function login(request: LoginRequest): Promise<LoginResponse> {
  const { data } = await api.post<LoginResponse>("/api/auth/login", request);
  return data;
}
