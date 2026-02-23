using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadPipe.Infrastructure.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class PlumbingUniqueIndexTwentySixFebruaryTwentySix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlumbingEntities_PhoneNumber_Date_Source",
                table: "PlumbingEntities");

            migrationBuilder.CreateIndex(
                name: "IX_PlumbingEntities_PhoneNumber_UnixDate_Source_MetaData",
                table: "PlumbingEntities",
                columns: new[] { "PhoneNumber", "UnixDate", "Source", "MetaData" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlumbingEntities_PhoneNumber_UnixDate_Source_MetaData",
                table: "PlumbingEntities");

            migrationBuilder.CreateIndex(
                name: "IX_PlumbingEntities_PhoneNumber_Date_Source",
                table: "PlumbingEntities",
                columns: new[] { "PhoneNumber", "Date", "Source" });
        }
    }
}
