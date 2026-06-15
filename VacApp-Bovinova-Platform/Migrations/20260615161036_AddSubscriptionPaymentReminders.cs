using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacApp_Bovinova_Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPaymentReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "reminder10-sent-for-renewal",
                table: "subscriptions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reminder5-sent-for-renewal",
                table: "subscriptions",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reminder10-sent-for-renewal",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "reminder5-sent-for-renewal",
                table: "subscriptions");
        }
    }
}
