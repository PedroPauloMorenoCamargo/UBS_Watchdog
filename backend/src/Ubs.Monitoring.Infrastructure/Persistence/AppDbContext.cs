using Microsoft.EntityFrameworkCore;

namespace Ubs.Monitoring.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // TODO: DbSet<Client> Clients { get; set; }
    // TODO: DbSet<Transaction> Transactions { get; set; }
    // TODO: DbSet<Alert> Alerts { get; set; }
}
