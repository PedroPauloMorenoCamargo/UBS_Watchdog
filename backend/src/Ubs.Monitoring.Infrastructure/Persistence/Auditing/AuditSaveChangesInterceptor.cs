using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ubs.Monitoring.Application.Common;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Infrastructure.Persistence.Auditing;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentRequestContext _ctx;

    public AuditSaveChangesInterceptor(ICurrentRequestContext ctx)
    {
        _ctx = ctx;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var db = eventData.Context;
        if (db is null) return result;

        AddAuditLogs(db);
        return result;
    }

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

    private void AddAuditLogs(DbContext db)
    {
        // ✅ REGRA FUNDAMENTAL:
        // Sem usuário autenticado → NÃO audita (seed, migration, job, startup, etc)
        if (_ctx.AnalystId is null)
            return;

        // Snapshot antes de adicionar logs (evita "Collection was modified")
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
            // Evita loop infinito
            if (entry.Entity is AuditLog)
                continue;

            var log = TryBuildAuditLog(entry);
            if (log is not null)
                logs.Add(log);
        }

        if (logs.Count > 0)
            db.Set<AuditLog>().AddRange(logs);
    }

    private AuditLog? TryBuildAuditLog(EntityEntry entry)
    {
        // Aqui já sabemos que AnalystId != null
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
        else // Update
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

    private static List<PropertyEntry> GetSerializableProps(EntityEntry entry, bool onlyModified)
    {
        return entry.Properties
            .Where(p =>
                !p.Metadata.IsPrimaryKey() &&
                !p.Metadata.IsShadowProperty() &&
                (!onlyModified || p.IsModified))
            .ToList();
    }

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
