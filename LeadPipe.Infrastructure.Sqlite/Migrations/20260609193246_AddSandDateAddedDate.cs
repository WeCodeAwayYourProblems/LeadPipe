using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadPipe.Infrastructure.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddSandDateAddedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SandEntities_CustardId",
                table: "SandEntities");

            migrationBuilder.AddColumn<string>(
                name: "DateAddedDate",
                table: "SandEntities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SandEntities_CustardId_DateAddedDate",
                table: "SandEntities",
                columns: new[] { "CustardId", "DateAddedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SandEntities_CustardId_DateAddedDate",
                table: "SandEntities");

            migrationBuilder.DropColumn(
                name: "DateAddedDate",
                table: "SandEntities");

            migrationBuilder.CreateIndex(
                name: "IX_SandEntities_CustardId",
                table: "SandEntities",
                column: "CustardId");
        }
    }
}
