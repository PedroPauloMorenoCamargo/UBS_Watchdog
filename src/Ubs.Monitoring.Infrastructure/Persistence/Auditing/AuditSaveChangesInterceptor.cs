using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ubs.Monitoring.Application.Common;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Infrastructure.Persistence.Auditing;

/// <summary>
/// Entity Framework Core interceptor responsible for automatically generating  audit log entries during <c>SaveChanges</c> operations.
/// </summary>
/// <remarks>
/// This interceptor captures create, update, and delete operations performed
/// by authenticated analysts and records before/after snapshots of entity data.
/// Auditing is skipped entirely when no authenticated analyst context is present (e.g. migrations, seeding, background jobs).
/// </remarks>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentRequestContext _ctx;

    public AuditSaveChangesInterceptor(ICurrentRequestContext ctx)
    {
        _ctx = ctx;
    }
    /// <summary>
    /// Intercepts synchronous <c>SaveChanges</c> calls to inject audit log creation.
    /// </summary>
    /// <param name="eventData">
    /// Contextual information about the current <see cref="DbContext"/> operation.
    /// </param>
    /// <param name="result">
    /// The current interception result for the save operation.
    /// </param>
    /// <returns>
    /// The (unmodified) interception result for the save operation.
    /// </returns>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var db = eventData.Context;
        if (db is null) return result;

        AddAuditLogs(db);
        return result;
    }
    /// <summary>
    /// Intercepts asynchronous <c>SaveChangesAsync</c> calls to inject audit log creation.
    /// </summary>
    /// <param name="eventData">
    /// Contextual information about the current <see cref="DbContext"/> operation.
    /// </param>
    /// <param name="result">
    /// The current interception result for the save operation.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing the (unmodified)
    /// interception result.
    /// </returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var db = eventData.Context;
        if (db is null) return ValueTask.FromResult(result);

        AddAuditLogs(db);
        return ValueTask.FromResult(result);
    }
    /// <summary>
    /// Scans tracked entity changes and appends corresponding audit log entries to the current database context.
    /// </summary>
    /// <remarks>
    /// Auditing is performed only when an authenticated analyst is present. AuditLog entities themselves are excluded to prevent infinite recursion.
    /// </remarks>
    /// <param name="db">
    /// The active <see cref="DbContext"/> instance.
    /// </param>
    private void AddAuditLogs(DbContext db)
    {
        if (_ctx.AnalystId is null)
            return;

        var entries = db.ChangeTracker
            .Entries()
            .Where(e =>
                e.State == EntityState.Added ||
                e.State == EntityState.Modified ||
                e.State == EntityState.Deleted)
            .ToList();

        if (entries.Count == 0)
            return;

        var logs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            if (entry.Entity is AuditLog)
                continue;

            var log = TryBuildAuditLog(entry);
            if (log is not null)
                logs.Add(log);
        }

        if (logs.Count > 0)
            db.Set<AuditLog>().AddRange(logs);
    }
    /// <summary>
    /// Attempts to construct an <see cref="AuditLog"/> entry from a tracked entity change.
    /// </summary>
    /// <param name="entry">
    /// The Entity Framework change-tracking entry.
    /// </param>
    /// <returns>
    /// A populated <see cref="AuditLog"/> if the entry represents a supported  state transition; otherwise, <c>null</c>.
    /// </returns>
    private AuditLog? TryBuildAuditLog(EntityEntry entry)
    {
        var analystId = _ctx.AnalystId!.Value;

        var pk = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        var entityId =
            pk?.CurrentValue?.ToString() ??
            pk?.OriginalValue?.ToString();

        if (string.IsNullOrWhiteSpace(entityId))
            return null;

        AuditAction? action = entry.State switch
        {
            EntityState.Added => AuditAction.Create,
            EntityState.Modified => AuditAction.Update,
            EntityState.Deleted => AuditAction.Delete,
            _ => null
        };

        if (action is null)
            return null;

        JsonDocument? before = null;
        JsonDocument? after = null;

        if (action == AuditAction.Create)
        {
            after = SerializeCurrent(entry, onlyModified: false);
        }
        else if (action == AuditAction.Delete)
        {
            before = SerializeOriginal(entry, onlyModified: false);
        }
        else 
        {
            before = SerializeOriginal(entry, onlyModified: true);
            after = SerializeCurrent(entry, onlyModified: true);
        }

        return new AuditLog(
            entityType: entry.Metadata.ClrType.Name,
            entityId: entityId,
            action: action.Value,
            performedByAnalystId: analystId,
            beforeJson: before,
            afterJson: after,
            correlationId: _ctx.CorrelationId
        );
    }
    /// <summary>
    /// Serializes original (pre-change) property values of an entity entry.
    /// </summary>
    /// <param name="entry">
    /// The Entity Framework change-tracking entry.
    /// </param>
    /// <param name="onlyModified">
    /// Indicates whether only modified properties should be included.
    /// </param>
    /// <returns>
    /// A <see cref="JsonDocument"/> containing serialized original values,  or <c>null</c> if no serializable properties exist or serialization fails.
    /// </returns>
    private static JsonDocument? SerializeOriginal(EntityEntry entry, bool onlyModified)
    {
        try
        {
            var props = GetSerializableProps(entry, onlyModified);
            if (props.Count == 0) return null;

            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in props)
                dict[p.Metadata.Name] = NormalizeValue(entry.OriginalValues[p.Metadata.Name]);

            return JsonSerializer.SerializeToDocument(dict);
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// Serializes current (post-change) property values of an entity entry.
    /// </summary>
    /// <param name="entry">
    /// The Entity Framework change-tracking entry.
    /// </param>
    /// <param name="onlyModified">
    /// Indicates whether only modified properties should be included.
    /// </param>
    /// <returns>
    /// A <see cref="JsonDocument"/> containing serialized current values, or <c>null</c> if no serializable properties exist or serialization fails.
    /// </returns>
    private static JsonDocument? SerializeCurrent(EntityEntry entry, bool onlyModified)
    {
        try
        {
            var props = GetSerializableProps(entry, onlyModified);
            if (props.Count == 0) return null;

            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in props)
                dict[p.Metadata.Name] = NormalizeValue(p.CurrentValue);

            return JsonSerializer.SerializeToDocument(dict);
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// Determines which entity properties are eligible for audit serialization.
    /// </summary>
    /// <param name="entry">
    /// The Entity Framework change-tracking entry.
    /// </param>
    /// <param name="onlyModified">
    /// Indicates whether only modified properties should be included.
    /// </param>
    /// <returns>
    /// A list of property entries eligible for serialization.
    /// </returns>
    private static List<PropertyEntry> GetSerializableProps(EntityEntry entry, bool onlyModified)
    {
        return entry.Properties
            .Where(p =>
                !p.Metadata.IsPrimaryKey() &&
                !p.Metadata.IsShadowProperty() &&
                (!onlyModified || p.IsModified))
            .ToList();
    }
    /// <summary>
    /// Normalizes values to ensure safe JSON serialization.
    /// </summary>
    /// <param name="value">
    /// The value to normalize.
    /// </param>
    /// <returns>
    /// A normalized value suitable for JSON serialization.
    /// </returns>
    private static object? NormalizeValue(object? value)
    {
        return value switch
        {
            JsonDocument jd => jd.RootElement.Clone(),
            JsonElement je => je.Clone(),
            _ => value
        };
    }
}
