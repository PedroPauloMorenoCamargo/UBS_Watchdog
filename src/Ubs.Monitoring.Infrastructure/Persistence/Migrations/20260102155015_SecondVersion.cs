using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ubs.Monitoring.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SecondVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "compliance_rules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_rules_Code",
                table: "compliance_rules",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rules_type_active",
                table: "compliance_rules",
                columns: new[] { "RuleType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "ix_rules_updated",
                table: "compliance_rules",
                column: "UpdatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "ix_clients_country",
                table: "clients",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "ix_clients_kyc_status",
                table: "clients",
                column: "KycStatus");

            migrationBuilder.CreateIndex(
                name: "ix_clients_risk_level",
                table: "clients",
                column: "RiskLevel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_compliance_rules_Code",
                table: "compliance_rules");

            migrationBuilder.DropIndex(
                name: "ix_rules_type_active",
                table: "compliance_rules");

            migrationBuilder.DropIndex(
                name: "ix_rules_updated",
                table: "compliance_rules");

            migrationBuilder.DropIndex(
                name: "ix_clients_country",
                table: "clients");

            migrationBuilder.DropIndex(
                name: "ix_clients_kyc_status",
                table: "clients");

            migrationBuilder.DropIndex(
                name: "ix_clients_risk_level",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "compliance_rules");
        }
    }
}
