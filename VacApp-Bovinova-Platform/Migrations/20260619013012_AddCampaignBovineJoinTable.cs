using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacApp_Bovinova_Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignBovineJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campaign-bovines",
                columns: table => new
                {
                    campaign_id = table.Column<int>(type: "int", nullable: false),
                    bovine_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p-k_campaign-bovines", x => new { x.campaign_id, x.bovine_id });
                    table.ForeignKey(
                        name: "f-k_campaign-bovines_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaign-bovines");
        }
    }
}
