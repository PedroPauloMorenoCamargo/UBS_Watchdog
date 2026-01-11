using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Application.Accounts;
using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.AccountIdentifiers;

/// <summary>
/// Service implementation for account identifier business operations.
/// </summary>
public sealed class AccountIdentifierService : IAccountIdentifierService
{
    private readonly IAccountRepository _accounts;
    private readonly ILogger<AccountIdentifierService> _logger;

    public AccountIdentifierService(
        IAccountRepository accounts,
        ILogger<AccountIdentifierService> logger)
    {
        _accounts = accounts;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all identifiers for a specific account.
    /// </summary>
    public async Task<(IReadOnlyList<AccountIdentifierDto>? Result, string? ErrorMessage)> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken ct)
    {
        var account = await _accounts.GetByIdWithDetailsAsync(accountId, ct);

        if (account is null)
        {
            _logger.LogWarning("Get identifiers failed: Account {AccountId} not found", accountId);
            return (null, $"Account with ID '{accountId}' not found.");
        }

        var identifiers = account.Identifiers.Select(MapToDto).ToList();


        return (identifiers, null);
    }

    /// <summary>
    /// Creates a new identifier for an account.
    /// </summary>
    public async Task<(AccountIdentifierDto? Result, string? ErrorMessage)> CreateIdentifierAsync(
        Guid accountId,
        CreateAccountIdentifierRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Creating identifier {IdentifierType} for account {AccountId}",
            request.IdentifierType, accountId);

        if (!await _accounts.ExistsAsync(accountId, ct))
        {
            _logger.LogWarning("Create identifier failed: Account {AccountId} not found", accountId);
            return (null, $"Account with ID '{accountId}' not found.");
        }

        var identifierValue = request.IdentifierValue?.Trim();
        if (string.IsNullOrWhiteSpace(identifierValue))
        {
            return (null, "Identifier value is required.");
        }

        if (await _accounts.IdentifierExistsAsync(accountId, request.IdentifierType, identifierValue, ct))
        {
            _logger.LogWarning("Create identifier failed: Identifier {IdentifierType} with value '{IdentifierValue}' already exists for account {AccountId}",
                request.IdentifierType, identifierValue, accountId);
            return (null, $"Identifier {request.IdentifierType} with value '{identifierValue}' already exists for this account.");
        }

        try
        {
            var identifier = new AccountIdentifier(
                accountId,
                request.IdentifierType,
                identifierValue,
                request.IssuedCountryCode?.Trim().ToUpperInvariant()
            );

            _accounts.AddIdentifier(identifier);
            await _accounts.SaveChangesAsync(ct);

            _logger.LogInformation("Identifier {IdentifierId} created successfully for account {AccountId}",
                identifier.Id, accountId);

            return (MapToDto(identifier), null);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Create identifier failed for account {AccountId}: {ErrorMessage}",
                accountId, ex.Message);
            return (null, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating identifier for account {AccountId}", accountId);
            throw;
        }
    }

    /// <summary>
    /// Removes an identifier from an account.
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> RemoveIdentifierAsync(
        Guid identifierId,
        CancellationToken ct)
    {
        _logger.LogInformation("Removing identifier {IdentifierId}", identifierId);

        var identifier = await _accounts.GetIdentifierByIdAsync(identifierId, ct);

        if (identifier is null)
        {
            _logger.LogWarning("Remove identifier failed: Identifier {IdentifierId} not found", identifierId);
            return (false, $"Account identifier with ID '{identifierId}' not found.");
        }

        _accounts.RemoveIdentifier(identifier);
        await _accounts.SaveChangesAsync(ct);

        _logger.LogInformation("Identifier {IdentifierId} removed successfully from account {AccountId}",
            identifierId, identifier.AccountId);

        return (true, null);
    }

    /// <summary>
    /// Maps a domain AccountIdentifier entity to a AccountIdentifierDto.
    /// </summary>
    private static AccountIdentifierDto MapToDto(AccountIdentifier identifier) =>
        new(
            Id: identifier.Id,
            IdentifierType: identifier.IdentifierType,
            IdentifierValue: identifier.IdentifierValue,
            IssuedCountryCode: identifier.IssuedCountryCode,
            CreatedAtUtc: identifier.CreatedAtUtc
        );
}
