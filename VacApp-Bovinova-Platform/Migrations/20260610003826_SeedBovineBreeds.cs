using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VacApp_Bovinova_Platform.Migrations
{
    /// <inheritdoc />
    public partial class SeedBovineBreeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bovine-breeds",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false),
                    mintemperature = table.Column<double>(name: "min-temperature", type: "double", nullable: false),
                    maxtemperature = table.Column<double>(name: "max-temperature", type: "double", nullable: false),
                    minheartrate = table.Column<int>(name: "min-heart-rate", type: "int", nullable: false),
                    maxheartrate = table.Column<int>(name: "max-heart-rate", type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_bovine-breeds", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "bovine-breeds",
                columns: new[] { "id", "max-heart-rate", "max-temperature", "min-heart-rate", "min-temperature", "name" },
                values: new object[,]
                {
                    { 1, 80, 39.299999999999997, 40, 38.0, "Holstein" },
                    { 2, 82, 39.399999999999999, 42, 38.0, "Jersey" },
                    { 3, 79, 39.299999999999997, 39, 37.899999999999999, "Brown Swiss" },
                    { 4, 75, 39.0, 35, 37.5, "Angus" },
                    { 5, 78, 39.200000000000003, 38, 37.799999999999997, "Hereford" },
                    { 6, 76, 39.100000000000001, 36, 37.5, "Charolais" },
                    { 7, 76, 39.100000000000001, 36, 37.600000000000001, "Limousin" },
                    { 8, 75, 39.0, 35, 37.5, "Wagyu" },
                    { 9, 78, 39.200000000000003, 38, 37.799999999999997, "Simmental" },
                    { 10, 85, 39.799999999999997, 45, 38.5, "Brahman" },
                    { 11, 84, 39.700000000000003, 44, 38.399999999999999, "Nelore" },
                    { 12, 83, 39.600000000000001, 43, 38.299999999999997, "Gyr" },
                    { 13, 85, 39.600000000000001, 35, 37.5, "Mestizo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bovine-breeds");
        }
    }
}
