/// <summary>
/// Request for authenticating an analyst.
/// </summary>
/// <param name="Email">The analyst's corporate email address.</param>
/// <param name="Password">The analyst's password.</param>
public sealed record LoginRequest(string Email, string Password);
