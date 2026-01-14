using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ubs.Monitoring.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_transactions_OccurredAtUtc",
                table: "transactions",
                newName: "IX_Transactions_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_ClientId_OccurredAtUtc",
                table: "transactions",
                newName: "IX_Transactions_ClientId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_AccountId_OccurredAtUtc",
                table: "transactions",
                newName: "IX_Transactions_AccountId_OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "transactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ClientId",
                table: "transactions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CpCountryCode",
                table: "transactions",
                column: "CpCountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CurrencyCode",
                table: "transactions",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransferMethod",
                table: "transactions",
                column: "TransferMethod");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Type",
                table: "transactions",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_AccountId",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ClientId",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CpCountryCode",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CurrencyCode",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransferMethod",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_Type",
                table: "transactions");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_OccurredAtUtc",
                table: "transactions",
                newName: "IX_transactions_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_ClientId_OccurredAtUtc",
                table: "transactions",
                newName: "IX_transactions_ClientId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_AccountId_OccurredAtUtc",
                table: "transactions",
                newName: "IX_transactions_AccountId_OccurredAtUtc");
        }
    }
}
