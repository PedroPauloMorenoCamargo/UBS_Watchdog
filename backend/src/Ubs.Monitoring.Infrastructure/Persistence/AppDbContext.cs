using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;
using Ubs.Monitoring.Infrastructure.Persistence.Seed;

namespace Ubs.Monitoring.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Analyst> Analysts => Set<Analyst>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountIdentifier> AccountIdentifiers => Set<AccountIdentifier>();
    public DbSet<FxRate> FxRates => Set<FxRate>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<ComplianceRule> ComplianceRules => Set<ComplianceRule>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<CaseFinding> CaseFindings => Set<CaseFinding>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // pgcrypto for gen_random_uuid()
        modelBuilder.HasPostgresExtension("pgcrypto");

        // PostgreSQL enum types (names must match your SQL)
        modelBuilder.HasPostgresEnum<LegalType>("legal_type");
        modelBuilder.HasPostgresEnum<RiskLevel>("risk_level");
        modelBuilder.HasPostgresEnum<KycStatus>("kyc_status");
        modelBuilder.HasPostgresEnum<AccountType>("account_type");
        modelBuilder.HasPostgresEnum<AccountStatus>("account_status");
        modelBuilder.HasPostgresEnum<IdentifierType>("identifier_type");
        modelBuilder.HasPostgresEnum<TransactionType>("transaction_type");
        modelBuilder.HasPostgresEnum<TransferMethod>("transfer_method");
        modelBuilder.HasPostgresEnum<RuleType>("rule_type");
        modelBuilder.HasPostgresEnum<Severity>("severity");
        modelBuilder.HasPostgresEnum<CaseStatus>("case_status");
        modelBuilder.HasPostgresEnum<CaseDecision>("case_decision");
        modelBuilder.HasPostgresEnum<AuditAction>("audit_action");

        // analysts
        modelBuilder.Entity<Analyst>(b =>
        {
            b.ToTable("analysts");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            b.Property(x => x.CorporateEmail).HasMaxLength(255).IsRequired();
            b.HasIndex(x => x.CorporateEmail).IsUnique();
            b.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            b.Property(x => x.PhoneNumber).HasMaxLength(30);
            b.Property(x => x.ProfilePictureBase64);
            b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("now()").IsRequired();
        });

        // clients
        modelBuilder.Entity<Client>(b =>
        {
            b.ToTable("clients");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.ContactNumber).HasMaxLength(30).IsRequired();
            b.Property(x => x.AddressJson).HasColumnType("jsonb").IsRequired();
            b.Property(x => x.CountryCode).HasColumnType("char(2)").IsRequired();
            b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("now()").IsRequired();
            b.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("now()").IsRequired();

            // Foreign key to countries
            b.HasOne<Country>()
                .WithMany()
                .HasForeignKey(x => x.CountryCode)
                .HasPrincipalKey(c => c.Code)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.CountryCode).HasDatabaseName("ix_clients_country");
            b.HasIndex(x => x.KycStatus).HasDatabaseName("ix_clients_kyc_status");
            b.HasIndex(x => x.RiskLevel).HasDatabaseName("ix_clients_risk_level");
        });

        // accounts
        modelBuilder.Entity<Account>(b =>
        {
            b.ToTable("accounts");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            b.Property(x => x.AccountIdentifier).HasMaxLength(80).IsRequired();
            b.HasIndex(x => x.AccountIdentifier).IsUnique();
            b.Property(x => x.CountryCode).HasColumnType("char(2)").IsRequired();
            b.Property(x => x.CurrencyCode).HasColumnType("char(3)").IsRequired();
            b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("now()").IsRequired();
            b.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("now()").IsRequired();

            b.HasOne(x => x.Client)
                .WithMany(c => c.Accounts)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure backing field for Identifiers collection
            b.HasMany(x => x.Identifiers)
                .WithOne(i => i.Account)
                .HasForeignKey(i => i.AccountId);
            b.Navigation(x => x.Identifiers).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // account_identifiers
        modelBuilder.Entity<AccountIdentifier>(b =>
        {
            b.ToTable("account_identifiers");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            b.Property(x => x.IdentifierValue).HasMaxLength(200).IsRequired();
            b.Property(x => x.IssuedCountryCode).HasColumnType("char(2)");
            b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("now()").IsRequired();

            // PIX + IBAN unique globally (partial unique)
            b.HasIndex(x => new { x.IdentifierType, x.IdentifierValue })
                .IsUnique()
                .HasDatabaseName("ux_unique_routing_identifiers")
                .HasFilter(
                    $"\"IdentifierType\" IN ({(int)IdentifierType.PIX_EMAIL}, " +
                    $"{(int)IdentifierType.PIX_PHONE}, " +
                    $"{(int)IdentifierType.PIX_RANDOM}, " +
                    $"{(int)IdentifierType.IBAN})"
            );

        });

        // fx_rates
        modelBuilder.Entity<FxRate>(b =>
        {
            b.ToTable("fx_rates");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            b.Property(x => x.BaseCurrencyCode).HasColumnType("char(3)").IsRequired();
            b.Property(x => x.QuoteCurrencyCode).HasColumnType("char(3)").IsRequired();
            b.Property(x => x.Rate).HasPrecision(18, 8).IsRequired();
            b.Property(x => x.AsOfUtc).IsRequired();

            b.HasIndex(x => new { x.BaseCurrencyCode, x.QuoteCurrencyCode, x.AsOfUtc })
                .IsUnique()
                .HasDatabaseName("ux_fx_rates_base_quote_asof");
        });

        // transactions
        modelBuilder.Entity<Transaction>(b =>
        {
            b.ToTable("transactions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

            b.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.BaseAmount).HasPrecision(18, 2).IsRequired();

            b.Property(x => x.CurrencyCode).HasColumnType("char(3)").IsRequired();
            b.Property(x => x.BaseCurrencyCode).HasColumnType("char(3)").IsRequired();

            b.Property(x => x.OccurredAtUtc).IsRequired();
            b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("now()").IsRequired();

            b.Property(x => x.CpName).HasMaxLength(200);
            b.Property(x => x.CpBank).HasMaxLength(200);
            b.Property(x => x.CpBranch).HasMaxLength(50);
            b.Property(x => x.CpAccount).HasMaxLength(80);
            b.Property(x => x.CpIdentifier).HasMaxLength(200);
            b.Property(x => x.CpCountryCode).HasColumnType("char(2)");

            b.HasOne(x => x.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Client)
                .WithMany(c => c.Transactions)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.FxRate)
                .WithMany()
                .HasForeignKey(x => x.FxRateId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => new { x.ClientId, x.OccurredAtUtc });
            b.HasIndex(x => new { x.AccountId, x.OccurredAtUtc });
            b.HasIndex(x => x.OccurredAtUtc);

            b.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "chk_transfer_required_fields",
                    @"
                    (""Type"" <> 2)
                    OR (
                        ""TransferMethod"" IS NOT NULL
                        AND ""CpCountryCode"" IS NOT NULL
                        AND ""CpIdentifierType"" IS NOT NULL
                        AND ""CpIdentifier"" IS NOT NULL
                    )
                    "
                );
            });

        });

        // compliance_rules
        modelBuilder.Entity<ComplianceRule>(b =>
        {
            b.ToTable("compliance_rules");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

            b.Property(x => x.Code).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();

            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.Scope).HasMaxLength(20);

            b.Property(x => x.ParametersJson).HasColumnType("jsonb").IsRequired();

            b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("now()").IsRequired();
            b.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("now()").IsRequired();

            b.HasIndex(x => new { x.RuleType, x.IsActive }).HasDatabaseName("ix_rules_type_active");
            b.HasIndex(x => x.UpdatedAtUtc).HasDatabaseName("ix_rules_updated");
        });


        // cases (1:1 with transactions)
        modelBuilder.Entity<Case>(b =>
        {
            b.ToTable("cases");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            b.Property(x => x.OpenedAtUtc).HasDefaultValueSql("now()").IsRequired();
            b.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("now()").IsRequired();

            b.HasOne(x => x.Transaction)
                .WithOne(t => t.Case)
                .HasForeignKey<Case>(x => x.TransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Client)
                .WithMany(c => c.Cases)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Account)
                .WithMany(a => a.Cases)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Analyst)
                .WithMany(a => a.Cases)
                .HasForeignKey(x => x.AnalystId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.TransactionId).IsUnique();
            b.HasIndex(x => new { x.Status, x.Severity }).HasDatabaseName("ix_cases_status_severity");
            b.HasIndex(x => new { x.ClientId, x.OpenedAtUtc }).HasDatabaseName("ix_cases_client_opened");
            b.HasIndex(x => x.UpdatedAtUtc).HasDatabaseName("ix_cases_updated");
        });

        // case_findings
        modelBuilder.Entity<CaseFinding>(b =>
        {
            b.ToTable("case_findings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            b.Property(x => x.EvidenceJson).HasColumnType("jsonb").IsRequired();
            b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("now()").IsRequired();

            b.HasOne(x => x.Case)
                .WithMany(c => c.Findings)
                .HasForeignKey(x => x.CaseId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Rule)
                .WithMany(r => r.CaseFindings)
                .HasForeignKey(x => x.RuleId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.CaseId).HasDatabaseName("ix_case_findings_case");
            b.HasIndex(x => x.RuleId).HasDatabaseName("ix_case_findings_rule");
        });

        // audit_logs
        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("audit_logs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            b.Property(x => x.EntityType).HasMaxLength(80).IsRequired();
            b.Property(x => x.EntityId).HasMaxLength(80).IsRequired();
            b.Property(x => x.CorrelationId).HasMaxLength(100);
            b.Property(x => x.BeforeJson).HasColumnType("jsonb");
            b.Property(x => x.AfterJson).HasColumnType("jsonb");
            b.Property(x => x.PerformedAtUtc).HasDefaultValueSql("now()").IsRequired();

            b.HasOne(x => x.PerformedByAnalyst)
                .WithMany(a => a.AuditLogs)
                .HasForeignKey(x => x.PerformedByAnalystId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.EntityType, x.EntityId }).HasDatabaseName("ix_audit_entity");
            b.HasIndex(x => x.PerformedAtUtc).HasDatabaseName("ix_audit_performed_at");
            b.HasIndex(x => new { x.PerformedByAnalystId, x.PerformedAtUtc }).HasDatabaseName("ix_audit_by_analyst_time");
        });

        // Apply all IEntityTypeConfiguration from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Seed data
        modelBuilder.SeedCountries();

        base.OnModelCreating(modelBuilder);
    }
}
