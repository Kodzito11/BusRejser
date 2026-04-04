using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusRejserLibrary.Migrations
{
    /// <inheritdoc />
    public partial class FixBusFacilitetJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_facilitet_bus_busId",
                table: "facilitet");

            migrationBuilder.DropIndex(
                name: "IX_facilitet_busId",
                table: "facilitet");

            migrationBuilder.DropColumn(
                name: "busId",
                table: "facilitet");

            migrationBuilder.CreateTable(
                name: "bus_facilitet",
                columns: table => new
                {
                    BusId = table.Column<int>(type: "int", nullable: false),
                    FacilitetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bus_facilitet", x => new { x.BusId, x.FacilitetId });
                    table.ForeignKey(
                        name: "FK_bus_facilitet_bus_BusId",
                        column: x => x.BusId,
                        principalTable: "bus",
                        principalColumn: "busId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bus_facilitet_facilitet_FacilitetId",
                        column: x => x.FacilitetId,
                        principalTable: "facilitet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_bus_facilitet_FacilitetId",
                table: "bus_facilitet",
                column: "FacilitetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bus_facilitet");

            migrationBuilder.AddColumn<int>(
                name: "busId",
                table: "facilitet",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_facilitet_busId",
                table: "facilitet",
                column: "busId");

            migrationBuilder.AddForeignKey(
                name: "FK_facilitet_bus_busId",
                table: "facilitet",
                column: "busId",
                principalTable: "bus",
                principalColumn: "busId");
        }
    }
}
