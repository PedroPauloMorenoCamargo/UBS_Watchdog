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

    /// <summary>
    /// Performs rule-type–specific structural validation of the provided parameters.
    /// </summary>
    /// <remarks>
    /// This method validates JSON structure, required fields, and value types  without performing any database access. It is intended to be executed
    /// before more expensive data-level validation.
    /// </remarks>
    /// <param name="ruleType">
    /// The type of compliance rule being validated.
    /// </param>
    /// <param name="parameters">
    /// The JSON object containing rule-specific parameters.
    /// </param>
    /// <returns>
    /// A read-only list of validation error messages. An empty list indicates that structural validation succeeded.
    /// </returns>
    private IReadOnlyList<string> ValidateStructure(RuleType ruleType, JsonElement parameters)
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
    /// <summary>
    /// Validates compliance rule parameters using both structural and  data-level validation rules.
    /// </summary>
    /// <remarks>
    /// Validation is executed in two phases:
    /// <list type="number">
    /// <item>
    /// <description>
    /// Structural validation (synchronous) to ensure correct JSON format, required fields, and value types.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Data validation (asynchronous) to verify parameter values against persisted data when applicable.
    /// </description>
    /// </item>
    /// </list>
    /// Data-level validation is skipped if structural validation fails.
    /// </remarks>
    /// <param name="ruleType">
    /// The type of compliance rule being validated.
    /// </param>
    /// <param name="parameters">
    /// The JSON object containing rule-specific parameters.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A read-only list of validation error messages. An empty list indicates that validation succeeded.
    /// </returns>
    public async Task<IReadOnlyList<string>> ValidateAsync(RuleType ruleType, JsonElement parameters, CancellationToken ct = default)
    {
        var errors = ValidateStructure(ruleType, parameters).ToList();

        if (errors.Count > 0)
            return errors;

        if (ruleType == RuleType.BannedCountries)
        {
            if (parameters.TryGetProperty("countries", out var countries) && countries.ValueKind == JsonValueKind.Array)
            {

                var countryCodesList = new List<string>();
                foreach (var c in countries.EnumerateArray())
                {
                    var countryCode = c.GetString()?.Trim().ToUpperInvariant();
                    if (!string.IsNullOrEmpty(countryCode))
                    {
                        countryCodesList.Add(countryCode);
                    }
                }


                if (countryCodesList.Count > 0)
                {
                    var existingCodes = await _countries.GetExistingCodesAsync(countryCodesList, ct);

                    foreach (var code in countryCodesList)
                    {
                        if (!existingCodes.Contains(code))
                        {
                            errors.Add($"BannedCountries: country code '{code}' does not exist in the system.");
                        }
                    }
                }
            }
        }

        return errors;
    }
}
