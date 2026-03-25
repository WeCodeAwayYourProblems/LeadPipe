using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadPipe.Infrastructure.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddLabelColumnToCaliperEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "CaliperEntities",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "CaliperEntities");
        }
    }
}
