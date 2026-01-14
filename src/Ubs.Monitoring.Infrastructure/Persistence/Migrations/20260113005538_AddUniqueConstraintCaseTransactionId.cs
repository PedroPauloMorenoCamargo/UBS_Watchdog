using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ubs.Monitoring.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintCaseTransactionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove duplicate cases if any exist before adding the constraint
            // Uses a CTE to identify and delete duplicates, keeping the oldest one (MIN created_at)
            migrationBuilder.Sql(@"
                WITH duplicates AS (
                    SELECT ""Id"",
                           ROW_NUMBER() OVER (PARTITION BY ""TransactionId"" ORDER BY ""OpenedAtUtc"" ASC) as rn
                    FROM cases
                )
                DELETE FROM case_findings
                WHERE ""CaseId"" IN (SELECT ""Id"" FROM duplicates WHERE rn > 1);

                WITH duplicates AS (
                    SELECT ""Id"",
                           ROW_NUMBER() OVER (PARTITION BY ""TransactionId"" ORDER BY ""OpenedAtUtc"" ASC) as rn
                    FROM cases
                )
                DELETE FROM cases
                WHERE ""Id"" IN (SELECT ""Id"" FROM duplicates WHERE rn > 1);
            ");

            // Drop the existing non-unique index if it exists
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_cases_TransactionId"";
            ");

            // Create the unique index
            migrationBuilder.CreateIndex(
                name: "IX_Cases_TransactionId_Unique",
                table: "cases",
                column: "TransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cases_TransactionId_Unique",
                table: "cases");

            // Recreate the non-unique index
            migrationBuilder.CreateIndex(
                name: "IX_cases_TransactionId",
                table: "cases",
                column: "TransactionId");
        }
    }
}
