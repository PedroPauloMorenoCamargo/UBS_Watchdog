import { api } from "@/lib/api";

/**
 * @typedef {Object} LoginRequest
 * @description Credentials payload sent to the authentication endpoint.
 * Used to authenticate an analyst with email and password.
 * @property {string} email - Analyst's corporate email address (required).
 * @property {string} password - Analyst's password in plain text (required, min 4 characters).
 */
export type LoginRequest = {
  email: string;
  password: string;
};

/**
 * @typedef {Object} AnalystProfile
 * @description Complete profile information of an authenticated analyst.
 * Returned by the backend after successful authentication.
 * @property {string} id - Unique identifier (GUID) of the analyst.
 * @property {string} corporateEmail - Corporate email address used for authentication.
 * @property {string} fullName - Analyst's full name for display purposes.
 * @property {string} [phoneNumber] - Optional contact phone number (nullable).
 * @property {string} [profilePictureBase64] - Optional base64-encoded profile picture (nullable).
 * @property {string} createdAtUtc - Account creation timestamp in UTC (ISO 8601 format).
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
 * @description Authentication response returned by the backend login endpoint.
 * Contains JWT token and complete analyst profile information.
 * @property {string} token - JWT authentication token (Bearer token format).
 * @property {string} expiresAtUtc - Token expiration timestamp in UTC (ISO 8601 format).
 * @property {AnalystProfile} analyst - Complete profile information of the authenticated analyst.
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
