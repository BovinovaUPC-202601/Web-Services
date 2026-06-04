using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace VacApp_Bovinova_Platform.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    bovine_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    alert_type = table.Column<string>(type: "longtext", nullable: false),
                    urgency_level = table.Column<string>(type: "longtext", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_alerts", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bovine-analyses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    bovine_id = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    visibleissues = table.Column<string>(name: "visible-issues", type: "text", nullable: false),
                    urgencylevel = table.Column<int>(name: "urgency-level", type: "int", nullable: false),
                    recommendation = table.Column<string>(type: "text", nullable: false),
                    confidence = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    createdat = table.Column<DateTime>(name: "created-at", type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_bovine-analyses", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bovine-chat-sessions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    bovine_id = table.Column<int>(type: "int", nullable: false),
                    messagesjson = table.Column<string>(name: "messages-json", type: "text", nullable: false),
                    createdat = table.Column<DateTime>(name: "created-at", type: "datetime(6)", nullable: false),
                    updatedat = table.Column<DateTime>(name: "updated-at", type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_bovine-chat-sessions", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "campaigns",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: false),
                    startdate = table.Column<DateTime>(name: "start-date", type: "datetime(6)", nullable: false),
                    enddate = table.Column<DateTime>(name: "end-date", type: "datetime(6)", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_campaigns", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    userid = table.Column<int>(name: "user-id", type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_categories", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "general-chat-sessions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    messagesjson = table.Column<string>(name: "messages-json", type: "text", nullable: false),
                    createdat = table.Column<DateTime>(name: "created-at", type: "datetime(6)", nullable: false),
                    updatedat = table.Column<DateTime>(name: "updated-at", type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_general-chat-sessions", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "stables",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    limit = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_stables", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    employee_status = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_staff", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    createdat = table.Column<DateTimeOffset>(name: "created-at", type: "datetime", nullable: true),
                    updatedat = table.Column<DateTimeOffset>(name: "updated-at", type: "datetime", nullable: true),
                    username = table.Column<string>(type: "longtext", nullable: false),
                    password = table.Column<string>(type: "longtext", nullable: false),
                    email = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_users", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    categoryid = table.Column<int>(name: "category-id", type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    userid = table.Column<int>(name: "user-id", type: "int", nullable: false),
                    expirationdate = table.Column<DateTime>(name: "expiration-date", type: "datetime(6)", nullable: true),
                    unit = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_products", x => x.id);
                    table.ForeignKey(
                        name: "f-k_products_categories_category-id",
                        column: x => x.categoryid,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bovines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    gender = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    birthdate = table.Column<DateTime>(name: "birth-date", type: "datetime(6)", nullable: false),
                    breed = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    stableid = table.Column<int>(name: "stable-id", type: "int", nullable: false),
                    bovineimg = table.Column<string>(name: "bovine-img", type: "varchar(300)", maxLength: 300, nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_bovines", x => x.id);
                    table.ForeignKey(
                        name: "f-k_bovines_-stable_stable-id",
                        column: x => x.stableid,
                        principalTable: "stables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bovine-health-records",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    bovine_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    deviceid = table.Column<string>(name: "device-id", type: "varchar(100)", maxLength: 100, nullable: false),
                    temperature = table.Column<float>(type: "float", nullable: false),
                    heartrate = table.Column<float>(name: "heart-rate", type: "float", nullable: false),
                    battery_level = table.Column<int>(type: "int", nullable: false),
                    isalert = table.Column<bool>(name: "is-alert", type: "tinyint(1)", nullable: false),
                    recordedat = table.Column<DateTime>(name: "recorded-at", type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_bovine-health-records", x => x.id);
                    table.ForeignKey(
                        name: "f-k_bovine-health-records_-bovine_bovine_id",
                        column: x => x.bovine_id,
                        principalTable: "bovines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "i-x_bovine-health-records_bovine_id",
                table: "bovine-health-records",
                column: "bovine_id");

            migrationBuilder.CreateIndex(
                name: "i-x_bovines_stable-id",
                table: "bovines",
                column: "stable-id");

            migrationBuilder.CreateIndex(
                name: "i-x_products_category-id",
                table: "products",
                column: "category-id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerts");

            migrationBuilder.DropTable(
                name: "bovine-analyses");

            migrationBuilder.DropTable(
                name: "bovine-chat-sessions");

            migrationBuilder.DropTable(
                name: "bovine-health-records");

            migrationBuilder.DropTable(
                name: "campaigns");

            migrationBuilder.DropTable(
                name: "general-chat-sessions");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "staff");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "bovines");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "stables");
        }
    }
}
