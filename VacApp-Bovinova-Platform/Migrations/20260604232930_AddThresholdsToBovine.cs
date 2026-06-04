using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacApp_Bovinova_Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddThresholdsToBovine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "max-heart-rate",
                table: "bovines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "max-temperature",
                table: "bovines",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "min-heart-rate",
                table: "bovines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "min-temperature",
                table: "bovines",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max-heart-rate",
                table: "bovines");

            migrationBuilder.DropColumn(
                name: "max-temperature",
                table: "bovines");

            migrationBuilder.DropColumn(
                name: "min-heart-rate",
                table: "bovines");

            migrationBuilder.DropColumn(
                name: "min-temperature",
                table: "bovines");
        }
    }
}
