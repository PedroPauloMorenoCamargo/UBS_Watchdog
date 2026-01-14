using System.Text.Json;
using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.ComplianceRules;


public static class ComplianceRuleMapper
{
    public static ComplianceRuleDto ToDto(ComplianceRule entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        using var doc = JsonDocument.Parse(entity.ParametersJson);
        var parameters = doc.RootElement.Clone();

        return new ComplianceRuleDto(
            entity.Id,
            entity.Code,
            entity.RuleType,
            entity.Name,
            entity.IsActive,
            entity.Severity,
            entity.Scope,
            parameters,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc
        );
    }
}
