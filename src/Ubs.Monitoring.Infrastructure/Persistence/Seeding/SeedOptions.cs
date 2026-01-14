namespace Ubs.Monitoring.Infrastructure.Persistence.Seeding;

public sealed class SeedOptions
{
    public bool Enabled { get; init; } = true;

    public bool SeedDemoData { get; init; } = true;

    public DefaultAnalystOptions DefaultAnalyst { get; init; } = new();

    public sealed class DefaultAnalystOptions
    {
        public string Email { get; init; } = "admin@ubs.com";
        public string Password { get; init; } = "admin123";
        public string FullName { get; init; } = "Admin Sistema UBS";
        public string? PhoneNumber { get; init; } = "+5511999999999";
    }
}
