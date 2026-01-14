using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ubs.Monitoring.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:account_status.account_status", "active,blocked,closed")
                .Annotation("Npgsql:Enum:account_type.account_type", "checking,savings,investment,other")
                .Annotation("Npgsql:Enum:audit_action.audit_action", "create,update,delete")
                .Annotation("Npgsql:Enum:case_decision.case_decision", "fraudulent,not_fraudulent,inconclusive")
                .Annotation("Npgsql:Enum:case_status.case_status", "new,under_review,resolved")
                .Annotation("Npgsql:Enum:identifier_type.identifier_type", "cpf,cnpj,tax_id,passport,lei,pix_email,pix_phone,pix_random,iban,other")
                .Annotation("Npgsql:Enum:kyc_status.kyc_status", "pending,verified,expired,rejected")
                .Annotation("Npgsql:Enum:legal_type.legal_type", "individual,corporate")
                .Annotation("Npgsql:Enum:risk_level.risk_level", "low,medium,high")
                .Annotation("Npgsql:Enum:rule_type.rule_type", "daily_limit,banned_countries,banned_accounts,structuring")
                .Annotation("Npgsql:Enum:severity.severity", "low,medium,high,critical")
                .Annotation("Npgsql:Enum:transaction_type.transaction_type", "deposit,withdrawal,transfer")
                .Annotation("Npgsql:Enum:transfer_method.transfer_method", "pix,ted,wire")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "analysts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CorporateEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ProfilePictureBase64 = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    LegalType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AddressJson = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    CountryCode = table.Column<string>(type: "char(2)", nullable: false),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    KycStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliance_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    RuleType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ParametersJson = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliance_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fx_rates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    BaseCurrencyCode = table.Column<string>(type: "char(3)", nullable: false),
                    QuoteCurrencyCode = table.Column<string>(type: "char(3)", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fx_rates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    EntityType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    PerformedByAnalystId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    BeforeJson = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    AfterJson = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_analysts_PerformedByAnalystId",
                        column: x => x.PerformedByAnalystId,
                        principalTable: "analysts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountIdentifier = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CountryCode = table.Column<string>(type: "char(2)", nullable: false),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_accounts_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "account_identifiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentifierType = table.Column<int>(type: "integer", nullable: false),
                    IdentifierValue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuedCountryCode = table.Column<string>(type: "char(2)", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_identifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_account_identifiers_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    TransferMethod = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", nullable: false),
                    BaseCurrencyCode = table.Column<string>(type: "char(3)", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FxRateId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CpName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CpBank = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CpBranch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CpAccount = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CpIdentifierType = table.Column<int>(type: "integer", nullable: true),
                    CpIdentifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CpCountryCode = table.Column<string>(type: "char(2)", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.Id);
                    table.CheckConstraint("chk_transfer_required_fields", "\r\n                    (\"Type\" <> 2)\r\n                    OR (\r\n                        \"TransferMethod\" IS NOT NULL\r\n                        AND \"CpCountryCode\" IS NOT NULL\r\n                        AND \"CpIdentifierType\" IS NOT NULL\r\n                        AND \"CpIdentifier\" IS NOT NULL\r\n                    )\r\n                    ");
                    table.ForeignKey(
                        name: "FK_transactions_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transactions_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transactions_fx_rates_FxRateId",
                        column: x => x.FxRateId,
                        principalTable: "fx_rates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: true),
                    AnalystId = table.Column<Guid>(type: "uuid", nullable: true),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    OpenedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cases_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cases_analysts_AnalystId",
                        column: x => x.AnalystId,
                        principalTable: "analysts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_cases_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cases_transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "case_findings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleType = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    EvidenceJson = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_findings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_case_findings_cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_case_findings_compliance_rules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "compliance_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_identifiers_AccountId",
                table: "account_identifiers",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "ux_unique_routing_identifiers",
                table: "account_identifiers",
                columns: new[] { "IdentifierType", "IdentifierValue" },
                unique: true,
                filter: "\"IdentifierType\" IN (5, 6, 7, 8)");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_AccountIdentifier",
                table: "accounts",
                column: "AccountIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_ClientId",
                table: "accounts",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_analysts_CorporateEmail",
                table: "analysts",
                column: "CorporateEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_audit_by_analyst_time",
                table: "audit_logs",
                columns: new[] { "PerformedByAnalystId", "PerformedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_entity",
                table: "audit_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_performed_at",
                table: "audit_logs",
                column: "PerformedAtUtc");

            migrationBuilder.CreateIndex(
                name: "ix_case_findings_case",
                table: "case_findings",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "ix_case_findings_rule",
                table: "case_findings",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_cases_AccountId",
                table: "cases",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_cases_AnalystId",
                table: "cases",
                column: "AnalystId");

            migrationBuilder.CreateIndex(
                name: "ix_cases_client_opened",
                table: "cases",
                columns: new[] { "ClientId", "OpenedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "ix_cases_status_severity",
                table: "cases",
                columns: new[] { "Status", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_cases_TransactionId",
                table: "cases",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cases_updated",
                table: "cases",
                column: "UpdatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "ux_fx_rates_base_quote_asof",
                table: "fx_rates",
                columns: new[] { "BaseCurrencyCode", "QuoteCurrencyCode", "AsOfUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transactions_AccountId_OccurredAtUtc",
                table: "transactions",
                columns: new[] { "AccountId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_ClientId_OccurredAtUtc",
                table: "transactions",
                columns: new[] { "ClientId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_FxRateId",
                table: "transactions",
                column: "FxRateId");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_OccurredAtUtc",
                table: "transactions",
                column: "OccurredAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_identifiers");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "case_findings");

            migrationBuilder.DropTable(
                name: "cases");

            migrationBuilder.DropTable(
                name: "compliance_rules");

            migrationBuilder.DropTable(
                name: "analysts");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "fx_rates");

            migrationBuilder.DropTable(
                name: "clients");
        }
    }
}
