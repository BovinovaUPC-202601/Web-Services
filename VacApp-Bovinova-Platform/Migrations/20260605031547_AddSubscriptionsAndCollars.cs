using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace VacApp_Bovinova_Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionsAndCollars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "users",
                type: "longtext",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "subscription-plan",
                table: "users",
                type: "longtext",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "additional-collar-requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    createdat = table.Column<DateTimeOffset>(name: "created-at", type: "datetime", nullable: true),
                    updatedat = table.Column<DateTimeOffset>(name: "updated-at", type: "datetime", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    monthly_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    requested_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_additional-collar-requests", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "collars",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    device_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    bovine_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    registered_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    lifecycle_status = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_collars", x => x.id);
                    table.ForeignKey(
                        name: "f-k_collars_-bovine_bovine_id",
                        column: x => x.bovine_id,
                        principalTable: "bovines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    createdat = table.Column<DateTimeOffset>(name: "created-at", type: "datetime", nullable: true),
                    updatedat = table.Column<DateTimeOffset>(name: "updated-at", type: "datetime", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    plan = table.Column<string>(type: "longtext", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    next_renewal = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    suspendedat = table.Column<DateTime>(name: "suspended-at", type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_subscriptions", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "i-x_collars_bovine_id",
                table: "collars",
                column: "bovine_id");

            migrationBuilder.CreateIndex(
                name: "i-x_collars_device_id",
                table: "collars",
                column: "device_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i-x_subscriptions_user_id",
                table: "subscriptions",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "additional-collar-requests");

            migrationBuilder.DropTable(
                name: "collars");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropColumn(
                name: "role",
                table: "users");

            migrationBuilder.DropColumn(
                name: "subscription-plan",
                table: "users");
        }
    }
}
