using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadPipe.Infrastructure.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceMediumCampaignContentTermToCornEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UtmCampaign",
                table: "CornEntities",
                type: "TEXT",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtmContent",
                table: "CornEntities",
                type: "TEXT",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtmMedium",
                table: "CornEntities",
                type: "TEXT",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtmSource",
                table: "CornEntities",
                type: "TEXT",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtmTerm",
                table: "CornEntities",
                type: "TEXT",
                maxLength: 45,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UtmCampaign",
                table: "CornEntities");

            migrationBuilder.DropColumn(
                name: "UtmContent",
                table: "CornEntities");

            migrationBuilder.DropColumn(
                name: "UtmMedium",
                table: "CornEntities");

            migrationBuilder.DropColumn(
                name: "UtmSource",
                table: "CornEntities");

            migrationBuilder.DropColumn(
                name: "UtmTerm",
                table: "CornEntities");
        }
    }
}
