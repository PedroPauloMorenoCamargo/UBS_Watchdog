using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ubs.Monitoring.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCountriesTableWithRiskLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    code = table.Column<string>(type: "varchar(2)", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", nullable: false),
                    risk_level = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.code);
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "AE", "United Arab Emirates" },
                    { "AR", "Argentina" },
                    { "AT", "Austria" },
                    { "AU", "Australia" },
                    { "BE", "Belgium" },
                    { "BO", "Bolivia" },
                    { "BR", "Brazil" },
                    { "CA", "Canada" },
                    { "CH", "Switzerland" },
                    { "CL", "Chile" }
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name", "risk_level" },
                values: new object[,]
                {
                    { "CN", "China", 1 },
                    { "CO", "Colombia", 1 }
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "DE", "Germany" },
                    { "DK", "Denmark" },
                    { "EC", "Ecuador" },
                    { "EG", "Egypt" },
                    { "ES", "Spain" },
                    { "FI", "Finland" },
                    { "FR", "France" },
                    { "GB", "United Kingdom" },
                    { "GR", "Greece" },
                    { "ID", "Indonesia" },
                    { "IE", "Ireland" },
                    { "IL", "Israel" },
                    { "IN", "India" }
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name", "risk_level" },
                values: new object[] { "IR", "Iran", 2 });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "IT", "Italy" },
                    { "JP", "Japan" },
                    { "KE", "Kenya" }
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name", "risk_level" },
                values: new object[] { "KP", "North Korea", 2 });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "KR", "South Korea" },
                    { "MX", "Mexico" },
                    { "MY", "Malaysia" }
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name", "risk_level" },
                values: new object[] { "NG", "Nigeria", 1 });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "NL", "Netherlands" },
                    { "NO", "Norway" },
                    { "NZ", "New Zealand" },
                    { "PE", "Peru" }
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name", "risk_level" },
                values: new object[] { "PH", "Philippines", 1 });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "PL", "Poland" },
                    { "PT", "Portugal" }
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name", "risk_level" },
                values: new object[] { "RU", "Russia", 2 });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "SA", "Saudi Arabia" },
                    { "SE", "Sweden" },
                    { "SG", "Singapore" }
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name", "risk_level" },
                values: new object[] { "SY", "Syria", 2 });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "TH", "Thailand" },
                    { "US", "United States" },
                    { "UY", "Uruguay" }
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name", "risk_level" },
                values: new object[] { "VE", "Venezuela", 2 });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "VN", "Vietnam" },
                    { "ZA", "South Africa" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_countries_name",
                table: "countries",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_countries_risk_level",
                table: "countries",
                column: "risk_level");

            migrationBuilder.AddForeignKey(
                name: "FK_clients_countries_CountryCode",
                table: "clients",
                column: "CountryCode",
                principalTable: "countries",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_clients_countries_CountryCode",
                table: "clients");

            migrationBuilder.DropTable(
                name: "countries");
        }
    }
}
