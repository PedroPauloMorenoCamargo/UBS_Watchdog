using System.Text.Json;
using Ubs.Monitoring.Application.Countries;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.ComplianceRules;

public sealed class ComplianceRuleParametersValidator : IComplianceRuleParametersValidator
{
    private readonly ICountryRepository _countries;

    public ComplianceRuleParametersValidator(ICountryRepository countries)
    {
        _countries = countries;
    }

    public IReadOnlyList<string> Validate(RuleType ruleType, JsonElement parameters)
    {
        var errors = new List<string>();

        switch (ruleType)
        {
            case RuleType.DailyLimit:
                if (!parameters.TryGetProperty("limitBaseAmount", out var limit) || limit.ValueKind != JsonValueKind.Number)
                    errors.Add("DailyLimit: 'limitBaseAmount' (number) is required.");
                else if (limit.TryGetDecimal(out var d) && d <= 0)
                    errors.Add("DailyLimit: 'limitBaseAmount' must be > 0.");
                break;

            case RuleType.BannedCountries:
                if (!parameters.TryGetProperty("countries", out var countries) || countries.ValueKind != JsonValueKind.Array)
                    errors.Add("BannedCountries: 'countries' (array) is required.");
                else
                {
                    foreach (var c in countries.EnumerateArray())
                    {
                        if (c.ValueKind != JsonValueKind.String)
                            errors.Add("BannedCountries: each country must be a string.");
                        else if ((c.GetString() ?? "").Trim().Length != 2)
                            errors.Add("BannedCountries: each country code must have exactly 2 characters (e.g., BR, US, DE)");
                    }
                }
                break;

            case RuleType.Structuring:
                if (!parameters.TryGetProperty("n", out var n) || n.ValueKind != JsonValueKind.Number || !n.TryGetInt32(out var nVal) || nVal <= 0)
                    errors.Add("Structuring: 'n' (int > 0) is required.");

                if (!parameters.TryGetProperty("xBaseAmount", out var x) || x.ValueKind != JsonValueKind.Number)
                    errors.Add("Structuring: 'xBaseAmount' (number) is required.");
                else if (x.TryGetDecimal(out var xVal) && xVal <= 0)
                    errors.Add("Structuring: 'xBaseAmount' must be > 0.");
                break;

            case RuleType.BannedAccounts:
                if (!parameters.TryGetProperty("entries", out var entries) || entries.ValueKind != JsonValueKind.Array)
                    errors.Add("BannedAccounts: 'entries' (array) is required.");
                break;

            default:
                errors.Add($"Unsupported rule type: {ruleType}");
                break;
        }

        return errors;
    }

    public async Task<IReadOnlyList<string>> ValidateAsync(RuleType ruleType, JsonElement parameters, CancellationToken ct = default)
    {
        var errors = Validate(ruleType, parameters).ToList();

        if (ruleType == RuleType.BannedCountries && errors.Count == 0)
        {
            if (parameters.TryGetProperty("countries", out var countries) && countries.ValueKind == JsonValueKind.Array)
            {
                foreach (var c in countries.EnumerateArray())
                {
                    var countryCode = c.GetString()?.Trim().ToUpperInvariant();
                    if (!string.IsNullOrEmpty(countryCode))
                    {
                        var exists = await _countries.ExistsAsync(countryCode, ct);
                        if (!exists)
                            errors.Add($"BannedCountries: country code '{countryCode}' does not exist in the system.");
                    }
                }
            }
        }

        return errors;
    }
}
