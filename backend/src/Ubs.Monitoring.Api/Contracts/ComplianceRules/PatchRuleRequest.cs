using Ubs.Monitoring.Domain.Enums;
namespace Ubs.Monitoring.Api.Contracts;
using System.Text.Json;
public sealed record PatchRuleRequest(
        string? Name,
        bool? IsActive,
        Severity? Severity,
        string? Scope,
        JsonElement? Parameters
);