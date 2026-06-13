using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacApp_Bovinova_Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffAccessManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "access_level",
                table: "staff",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "staff",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "linked_user_id",
                table: "staff",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "i-x_staff_user_id_linked_user_id",
                table: "staff",
                columns: new[] { "user_id", "linked_user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i-x_staff_user_id_linked_user_id",
                table: "staff");

            migrationBuilder.DropColumn(
                name: "access_level",
                table: "staff");

            migrationBuilder.DropColumn(
                name: "email",
                table: "staff");

            migrationBuilder.DropColumn(
                name: "linked_user_id",
                table: "staff");
        }
    }
}
