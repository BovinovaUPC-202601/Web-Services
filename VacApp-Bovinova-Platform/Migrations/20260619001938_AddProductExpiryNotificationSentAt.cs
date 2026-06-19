using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacApp_Bovinova_Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddProductExpiryNotificationSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "expiry-notification-sent-at",
                table: "products",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expiry-notification-sent-at",
                table: "products");
        }
    }
}
