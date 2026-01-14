namespace Ubs.Monitoring.Api.Contracts;

/// <summary>
/// Successful authentication response.
/// </summary>
/// <param name="Token">JWT bearer token.</param>
/// <param name="ExpiresAtUtc">Token expiration in UTC.</param>
/// <param name="Analyst">Authenticated analyst profile.</param>
public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAtUtc,
    AnalystProfileResponse Analyst
);

/// <summary>
/// Request for authenticating an analyst.
/// </summary>
/// <param name="Email">The analyst's corporate email address.</param>
/// <param name="Password">The analyst's password.</param>
public sealed record LoginRequest(string Email, string Password);
