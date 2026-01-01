namespace Ubs.Monitoring.Infrastructure.Persistence.Seeding;

public sealed class SeedOptions
{
    public DefaultAnalystOptions DefaultAnalyst { get; init; } = new();

    public sealed class DefaultAnalystOptions
    {
        public string Email { get; init; } = "analyst@ubs.com";
        public string Password { get; init; } = "Password123!";
        public string FullName { get; init; } = "Default Analyst";
    }
}
