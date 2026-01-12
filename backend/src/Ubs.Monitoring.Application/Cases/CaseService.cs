using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Application.Auth;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Cases;

/// <summary>
/// Service implementation for case business operations.
/// </summary>
public sealed class CaseService : ICaseService
{
    private readonly ICaseRepository _cases;
    private readonly IAnalystRepository _analysts;
    private readonly ILogger<CaseService> _logger;

    public CaseService(
        ICaseRepository cases,
        IAnalystRepository analysts,
        ILogger<CaseService> logger)
    {
        _cases = cases;
        _analysts = analysts;
        _logger = logger;
    }

    public async Task<PagedResult<CaseResponseDto>> GetPagedAsync(
        CaseFilterRequest filter,
        CancellationToken ct)
    {
        _logger.LogDebug("Retrieving cases with filter: {@Filter}", filter);

        var (items, totalCount) = await _cases.GetPagedAsync(filter, ct);

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var dtos = items.Select(MapToResponseDto).ToList();

        _logger.LogDebug("Retrieved {Count} cases out of {Total}", items.Count, totalCount);

        return PagedResult<CaseResponseDto>.Create(dtos, page, pageSize, totalCount);
    }

    public async Task<CaseDetailDto?> GetByIdAsync(Guid caseId, CancellationToken ct)
    {
        _logger.LogDebug("Retrieving case {CaseId}", caseId);

        var caseEntity = await _cases.GetByIdWithDetailsAsync(caseId, ct);

        if (caseEntity is null)
        {
            _logger.LogWarning("Case {CaseId} not found", caseId);
            return null;
        }

        _logger.LogDebug("Retrieved case {CaseId} with {FindingsCount} findings",
            caseId, caseEntity.Findings.Count);

        return MapToDetailDto(caseEntity);
    }

    public async Task<IReadOnlyList<CaseFindingDto>?> GetFindingsAsync(
        Guid caseId,
        CaseFindingFilterRequest filter,
        CancellationToken ct)
    {
        _logger.LogDebug("Retrieving findings for case {CaseId}", caseId);

        if (!await _cases.ExistsAsync(caseId, ct))
        {
            _logger.LogWarning("Case {CaseId} not found when retrieving findings", caseId);
            return null;
        }

        var findings = await _cases.GetFindingsByCaseIdAsync(caseId, filter, ct);

        var dtos = findings.Select(f => new CaseFindingDto(
            Id: f.Id,
            CaseId: f.CaseId,
            RuleId: f.RuleId,
            RuleName: f.Rule?.Name ?? "Unknown",
            RuleCode: f.Rule?.Code ?? "Unknown",
            RuleType: f.RuleType,
            Severity: f.Severity,
            EvidenceJson: f.EvidenceJson,
            CreatedAtUtc: f.CreatedAtUtc
        )).ToList();

        _logger.LogDebug("Retrieved {Count} findings for case {CaseId}", dtos.Count, caseId);

        return dtos;
    }

    public async Task<(CaseResponseDto? Result, string? ErrorMessage)> UpdateAsync(
        Guid caseId,
        UpdateCaseRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Updating case {CaseId} with request: {@Request}", caseId, request);

        var caseEntity = await _cases.GetForUpdateAsync(caseId, ct);

        if (caseEntity is null)
        {
            _logger.LogWarning("Update failed: Case {CaseId} not found", caseId);
            return (null, $"Case with ID '{caseId}' not found.");
        }

        try
        {
            // Handle analyst assignment
            if (request.AnalystId.HasValue)
            {
                var analyst = await _analysts.GetByIdAsync(request.AnalystId.Value, ct);
                if (analyst is null)
                {
                    return (null, $"Analyst with ID '{request.AnalystId.Value}' not found.");
                }

                if (caseEntity.AnalystId.HasValue)
                {
                    caseEntity.ReassignAnalyst(request.AnalystId.Value);
                }
                else
                {
                    caseEntity.AssignAnalyst(request.AnalystId.Value);
                }
            }

            // Handle status change
            if (request.Status.HasValue)
            {
                switch (request.Status.Value)
                {
                    case CaseStatus.Resolved:
                        if (!request.Decision.HasValue)
                        {
                            return (null, "A decision is required to resolve the case.");
                        }
                        caseEntity.Resolve(request.Decision.Value);
                        break;

                    case CaseStatus.UnderReview:
                        if (caseEntity.Status == CaseStatus.Resolved)
                        {
                            caseEntity.Reopen();
                        }
                        else if (caseEntity.Status == CaseStatus.New && !caseEntity.AnalystId.HasValue)
                        {
                            return (null, "Cannot move to UnderReview without assigning an analyst.");
                        }
                        break;

                    case CaseStatus.New:
                        return (null, "Cannot transition back to New status.");
                }
            }
            // Handle decision without explicit status change (auto-resolve)
            else if (request.Decision.HasValue && caseEntity.Status != CaseStatus.Resolved)
            {
                caseEntity.Resolve(request.Decision.Value);
            }

            await _cases.SaveChangesAsync(ct);

            // Reload for response
            var updated = await _cases.GetByIdAsync(caseId, ct);
            var dto = MapToResponseDto(updated!);

            _logger.LogInformation("Case {CaseId} updated successfully. New status: {Status}",
                caseId, updated!.Status);

            return (dto, null);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Update failed for case {CaseId}: {Message}", caseId, ex.Message);
            return (null, ex.Message);
        }
    }

    public async Task<(CaseResponseDto? Result, string? ErrorMessage)> AssignToAnalystAsync(
        Guid caseId,
        Guid analystId,
        CancellationToken ct)
    {
        _logger.LogInformation("Assigning case {CaseId} to analyst {AnalystId}", caseId, analystId);

        var analyst = await _analysts.GetByIdAsync(analystId, ct);
        if (analyst is null)
        {
            _logger.LogWarning("Assign failed: Analyst {AnalystId} not found", analystId);
            return (null, $"Analyst with ID '{analystId}' not found.");
        }

        var caseEntity = await _cases.GetForUpdateAsync(caseId, ct);
        if (caseEntity is null)
        {
            _logger.LogWarning("Assign failed: Case {CaseId} not found", caseId);
            return (null, $"Case with ID '{caseId}' not found.");
        }

        try
        {
            if (caseEntity.AnalystId.HasValue)
            {
                caseEntity.ReassignAnalyst(analystId);
            }
            else
            {
                caseEntity.AssignAnalyst(analystId);
            }

            await _cases.SaveChangesAsync(ct);

            // Reload for response
            var updated = await _cases.GetByIdAsync(caseId, ct);
            var dto = MapToResponseDto(updated!);

            _logger.LogInformation("Case {CaseId} assigned to analyst {AnalystId}. Status: {Status}",
                caseId, analystId, updated!.Status);

            return (dto, null);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Assign failed for case {CaseId}: {Message}", caseId, ex.Message);
            return (null, ex.Message);
        }
    }

    private static CaseResponseDto MapToResponseDto(Domain.Entities.Case c) => new(
        Id: c.Id,
        TransactionId: c.TransactionId,
        ClientId: c.ClientId,
        ClientName: c.Client?.Name ?? "Unknown",
        AccountId: c.AccountId,
        AccountIdentifier: c.Account?.AccountIdentifier ?? "Unknown",
        Status: c.Status,
        Decision: c.Decision,
        AnalystId: c.AnalystId,
        AnalystName: c.Analyst?.FullName,
        Severity: c.Severity,
        FindingsCount: c.Findings.Count,
        OpenedAtUtc: c.OpenedAtUtc,
        UpdatedAtUtc: c.UpdatedAtUtc,
        ResolvedAtUtc: c.ResolvedAtUtc
    );

    private static CaseDetailDto MapToDetailDto(Domain.Entities.Case c) => new(
        Id: c.Id,
        TransactionId: c.TransactionId,
        Transaction: c.Transaction is not null ? new TransactionSummaryDto(
            Id: c.Transaction.Id,
            Type: c.Transaction.Type,
            TransferMethod: c.Transaction.TransferMethod,
            Amount: c.Transaction.Amount,
            CurrencyCode: c.Transaction.CurrencyCode,
            BaseAmount: c.Transaction.BaseAmount,
            BaseCurrencyCode: c.Transaction.BaseCurrencyCode,
            OccurredAtUtc: c.Transaction.OccurredAtUtc
        ) : null!,
        ClientId: c.ClientId,
        Client: c.Client is not null ? new ClientSummaryDto(
            Id: c.Client.Id,
            Name: c.Client.Name,
            LegalType: c.Client.LegalType,
            CountryCode: c.Client.CountryCode,
            RiskLevel: c.Client.RiskLevel,
            KycStatus: c.Client.KycStatus
        ) : null!,
        AccountId: c.AccountId,
        Account: c.Account is not null ? new AccountSummaryDto(
            Id: c.Account.Id,
            AccountIdentifier: c.Account.AccountIdentifier,
            AccountType: c.Account.AccountType,
            CountryCode: c.Account.CountryCode,
            CurrencyCode: c.Account.CurrencyCode,
            Status: c.Account.Status
        ) : null!,
        Status: c.Status,
        Decision: c.Decision,
        AnalystId: c.AnalystId,
        Analyst: c.Analyst is not null ? new AnalystSummaryDto(
            Id: c.Analyst.Id,
            FullName: c.Analyst.FullName,
            CorporateEmail: c.Analyst.CorporateEmail
        ) : null,
        Severity: c.Severity,
        Findings: c.Findings.Select(f => new CaseFindingDto(
            Id: f.Id,
            CaseId: f.CaseId,
            RuleId: f.RuleId,
            RuleName: f.Rule?.Name ?? "Unknown",
            RuleCode: f.Rule?.Code ?? "Unknown",
            RuleType: f.RuleType,
            Severity: f.Severity,
            EvidenceJson: f.EvidenceJson,
            CreatedAtUtc: f.CreatedAtUtc
        )).ToList(),
        OpenedAtUtc: c.OpenedAtUtc,
        UpdatedAtUtc: c.UpdatedAtUtc,
        ResolvedAtUtc: c.ResolvedAtUtc
    );
}
