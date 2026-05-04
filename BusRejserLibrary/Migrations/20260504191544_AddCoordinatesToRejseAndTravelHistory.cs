using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusRejserLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinatesToRejseAndTravelHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "travel_history",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "travel_history",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "rejse",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "rejse",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "travel_history");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "travel_history");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "rejse");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "rejse");
        }
    }
}
