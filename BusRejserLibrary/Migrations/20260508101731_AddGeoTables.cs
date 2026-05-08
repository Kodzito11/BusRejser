using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusRejserLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "geoname_places",
                columns: table => new
                {
                    geoname_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ascii_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    country_code = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    admin1_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    latitude = table.Column<double>(type: "double", nullable: true),
                    longitude = table.Column<double>(type: "double", nullable: true),
                    population = table.Column<long>(type: "bigint", nullable: false),
                    feature_class = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    feature_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geoname_places", x => x.geoname_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "geo_alternate_names",
                columns: table => new
                {
                    GeoAlternateNameId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GeoNameId = table.Column<int>(type: "int", nullable: false),
                    AlternateName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsoLanguage = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPreferredName = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsShortName = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geo_alternate_names", x => x.GeoAlternateNameId);
                    table.ForeignKey(
                        name: "FK_geo_alternate_names_geoname_places_GeoNameId",
                        column: x => x.GeoNameId,
                        principalTable: "geoname_places",
                        principalColumn: "geoname_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_geo_alternate_names_GeoNameId",
                table: "geo_alternate_names",
                column: "GeoNameId");

            migrationBuilder.CreateIndex(
                name: "IX_geoname_places_ascii_name",
                table: "geoname_places",
                column: "ascii_name");

            migrationBuilder.CreateIndex(
                name: "IX_geoname_places_country_code",
                table: "geoname_places",
                column: "country_code");

            migrationBuilder.CreateIndex(
                name: "IX_geoname_places_country_code_admin1_code",
                table: "geoname_places",
                columns: new[] { "country_code", "admin1_code" });

            migrationBuilder.CreateIndex(
                name: "IX_geoname_places_name",
                table: "geoname_places",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "geo_alternate_names");

            migrationBuilder.DropTable(
                name: "geoname_places");
        }
    }
}
