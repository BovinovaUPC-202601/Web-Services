using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacApp_Bovinova_Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToBovineBreeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "user-id",
                table: "bovine-breeds",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 1,
                column: "user-id",
                value: null);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 2,
                column: "user-id",
                value: null);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 3,
                column: "user-id",
                value: null);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 4,
                column: "user-id",
                value: null);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 5,
                column: "user-id",
                value: null);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 6,
                column: "user-id",
                value: null);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 7,
                column: "user-id",
                value: null);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 8,
                column: "user-id",
                value: null);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 9,
                column: "user-id",
                value: null);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 10,
                column: "user-id",
                value: null);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 11,
                column: "user-id",
                value: null);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 12,
                column: "user-id",
                value: null);

            migrationBuilder.UpdateData(
                table: "bovine-breeds",
                keyColumn: "id",
                keyValue: 13,
                column: "user-id",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user-id",
                table: "bovine-breeds");
        }
    }
}
