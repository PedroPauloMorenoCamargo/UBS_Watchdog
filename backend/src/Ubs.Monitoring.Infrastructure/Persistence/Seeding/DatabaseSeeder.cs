using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Infrastructure.Persistence;

namespace Ubs.Monitoring.Infrastructure.Persistence.Seeding;

public sealed class DatabaseSeeder
{
    private readonly AppDbContext _db;
    private readonly SeedOptions _options;

    public DatabaseSeeder(AppDbContext db, IOptions<SeedOptions> options)
    {
        _db = db;
        _options = options.Value;
    }
    /// <summary>
    /// Executes the database seeding process.
    /// </summary>
    /// <param name="ct">
    /// A cancellation token used to cancel the operation if application shutdown is requested.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous seeding operation.
    /// </returns>
    /// <remarks>
    /// This method performs the following steps:
    /// <list type="number">
    ///   <item>Checks whether any analysts already exist.</item>
    ///   <item>Reads default analyst configuration values.</item>
    ///   <item>Hashes the configured password using BCrypt.</item>
    ///   <item>Persists the default analyst to the database.</item>
    /// </list>
    /// If any analyst is already present, the method exits without making changes.
    /// </remarks>
    public async Task SeedAsync(CancellationToken ct)
    {
        var anyAnalyst = await _db.Analysts.AsNoTracking().AnyAsync(ct);
        if (anyAnalyst) return;

        var email = (_options.DefaultAnalyst.Email ?? "").Trim().ToLowerInvariant();
        var password = _options.DefaultAnalyst.Password ?? "";
        var fullName = _options.DefaultAnalyst.FullName ?? "Default Analyst";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var analyst = new Analyst
        {
            CorporateEmail = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName,
            PhoneNumber = null,
            ProfilePictureBase64 = null,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Analysts.Add(analyst);
        await _db.SaveChangesAsync(ct);
    }
}
