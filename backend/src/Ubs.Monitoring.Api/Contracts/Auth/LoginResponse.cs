namespace Ubs.Monitoring.Api.Contracts;

public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAtUtc,
    AnalystProfileResponse Analyst
);
