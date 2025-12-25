using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadPipe.Infrastructure.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToCallEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CallEntities_PhoneNumber_CallDate",
                table: "CallEntities",
                columns: new[] { "PhoneNumber", "CallDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CallEntities_PhoneNumber_CallDate",
                table: "CallEntities");
        }
    }
}
