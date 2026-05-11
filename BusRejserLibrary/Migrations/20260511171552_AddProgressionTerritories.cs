using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusRejserLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddProgressionTerritories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProgressionTerritoryId",
                table: "rejse",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "progression_territories",
                columns: table => new
                {
                    ProgressionTerritoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsVisible = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsComingSoon = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MasteryTarget = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_progression_territories", x => x.ProgressionTerritoryId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "progression_territory_aliases",
                columns: table => new
                {
                    ProgressionTerritoryAliasId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProgressionTerritoryId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_progression_territory_aliases", x => x.ProgressionTerritoryAliasId);
                    table.ForeignKey(
                        name: "FK_progression_territory_aliases_progression_territories_Progre~",
                        column: x => x.ProgressionTerritoryId,
                        principalTable: "progression_territories",
                        principalColumn: "ProgressionTerritoryId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rejse_ProgressionTerritoryId",
                table: "rejse",
                column: "ProgressionTerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_progression_territories_Key",
                table: "progression_territories",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_progression_territory_aliases_ProgressionTerritoryId_Value",
                table: "progression_territory_aliases",
                columns: new[] { "ProgressionTerritoryId", "Value" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_rejse_progression_territories_ProgressionTerritoryId",
                table: "rejse",
                column: "ProgressionTerritoryId",
                principalTable: "progression_territories",
                principalColumn: "ProgressionTerritoryId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_rejse_progression_territories_ProgressionTerritoryId",
                table: "rejse");

            migrationBuilder.DropTable(
                name: "progression_territory_aliases");

            migrationBuilder.DropTable(
                name: "progression_territories");

            migrationBuilder.DropIndex(
                name: "IX_rejse_ProgressionTerritoryId",
                table: "rejse");

            migrationBuilder.DropColumn(
                name: "ProgressionTerritoryId",
                table: "rejse");
        }
    }
}
