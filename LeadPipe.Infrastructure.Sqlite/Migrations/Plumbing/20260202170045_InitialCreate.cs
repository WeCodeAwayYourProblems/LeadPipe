using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadPipe.Infrastructure.Sqlite.Migrations.Plumbing
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaliperEntities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    PhoneNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnixDate = table.Column<long>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    Duration = table.Column<long>(type: "INTEGER", nullable: false),
                    Billable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaliperEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CornEntities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    PhoneNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnixDate = table.Column<long>(type: "INTEGER", nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                    MetaData = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CornEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustardEntities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    PhoneNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    PhoneNumber2 = table.Column<long>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnixDate = table.Column<long>(type: "INTEGER", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnixCancelDate = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustardEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlumbingEntities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhoneNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnixDate = table.Column<long>(type: "INTEGER", nullable: false),
                    Contents = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    MetaData = table.Column<string>(type: "TEXT", nullable: false),
                    Branch = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlumbingEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncState",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessId = table.Column<string>(type: "TEXT", nullable: false),
                    LastProcessedId = table.Column<string>(type: "TEXT", nullable: true),
                    LastSyncUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnixLastSyncUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CornCaliperLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CornId = table.Column<long>(type: "INTEGER", nullable: false),
                    CaliperId = table.Column<long>(type: "INTEGER", nullable: false),
                    MatchingPhone = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CornCaliperLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CornCaliperLinks_CaliperEntities_CaliperId",
                        column: x => x.CaliperId,
                        principalTable: "CaliperEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CornCaliperLinks_CornEntities_CornId",
                        column: x => x.CornId,
                        principalTable: "CornEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustardCaliperLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustardId = table.Column<long>(type: "INTEGER", nullable: false),
                    CaliperId = table.Column<long>(type: "INTEGER", nullable: false),
                    MatchingPhone = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustardCaliperLinks", x => x.Id);
                    table.CheckConstraint("CK_CustardCaliper_MatchingPhone", "MatchingPhone <> 0");
                    table.ForeignKey(
                        name: "FK_CustardCaliperLinks_CaliperEntities_CaliperId",
                        column: x => x.CaliperId,
                        principalTable: "CaliperEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustardCaliperLinks_CustardEntities_CustardId",
                        column: x => x.CustardId,
                        principalTable: "CustardEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustardCornLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustardId = table.Column<long>(type: "INTEGER", nullable: false),
                    CornId = table.Column<long>(type: "INTEGER", nullable: false),
                    MatchingPhone = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustardCornLinks", x => x.Id);
                    table.CheckConstraint("CK_CustardCorn_MatchingPhone", "MatchingPhone <> 0");
                    table.ForeignKey(
                        name: "FK_CustardCornLinks_CornEntities_CornId",
                        column: x => x.CornId,
                        principalTable: "CornEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustardCornLinks_CustardEntities_CustardId",
                        column: x => x.CustardId,
                        principalTable: "CustardEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SandEntities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    CustardId = table.Column<long>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnixDate = table.Column<long>(type: "INTEGER", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnixCancelDate = table.Column<long>(type: "INTEGER", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    Complete = table.Column<bool>(type: "INTEGER", nullable: false),
                    Value = table.Column<decimal>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Seller = table.Column<int>(type: "INTEGER", nullable: false),
                    Seller2 = table.Column<int>(type: "INTEGER", nullable: false),
                    Seller3 = table.Column<int>(type: "INTEGER", nullable: false),
                    Offerman = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SandEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SandEntities_CustardEntities_CustardId",
                        column: x => x.CustardId,
                        principalTable: "CustardEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CornPlumbingLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CornId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlumbingId = table.Column<long>(type: "INTEGER", nullable: false),
                    MatchingPhone = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CornPlumbingLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CornPlumbingLinks_CornEntities_CornId",
                        column: x => x.CornId,
                        principalTable: "CornEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CornPlumbingLinks_PlumbingEntities_PlumbingId",
                        column: x => x.PlumbingId,
                        principalTable: "PlumbingEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustardPlumbingLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustardId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlumbingId = table.Column<long>(type: "INTEGER", nullable: false),
                    MatchingPhone = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustardPlumbingLinks", x => x.Id);
                    table.CheckConstraint("CK_CustardPlumbing_MatchingPhone", "MatchingPhone <> 0");
                    table.ForeignKey(
                        name: "FK_CustardPlumbingLinks_CustardEntities_CustardId",
                        column: x => x.CustardId,
                        principalTable: "CustardEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustardPlumbingLinks_PlumbingEntities_PlumbingId",
                        column: x => x.PlumbingId,
                        principalTable: "PlumbingEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlumbingCaliperLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlumbingId = table.Column<long>(type: "INTEGER", nullable: false),
                    CaliperId = table.Column<long>(type: "INTEGER", nullable: false),
                    MatchingPhone = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlumbingCaliperLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlumbingCaliperLinks_CaliperEntities_CaliperId",
                        column: x => x.CaliperId,
                        principalTable: "CaliperEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlumbingCaliperLinks_PlumbingEntities_PlumbingId",
                        column: x => x.PlumbingId,
                        principalTable: "PlumbingEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SandCaliperLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SandId = table.Column<long>(type: "INTEGER", nullable: false),
                    CaliperId = table.Column<long>(type: "INTEGER", nullable: false),
                    MatchingPhone = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SandCaliperLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SandCaliperLinks_CaliperEntities_CaliperId",
                        column: x => x.CaliperId,
                        principalTable: "CaliperEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SandCaliperLinks_SandEntities_SandId",
                        column: x => x.SandId,
                        principalTable: "SandEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SandCornLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SandId = table.Column<long>(type: "INTEGER", nullable: false),
                    CornId = table.Column<long>(type: "INTEGER", nullable: false),
                    MatchingPhone = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SandCornLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SandCornLinks_CornEntities_CornId",
                        column: x => x.CornId,
                        principalTable: "CornEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SandCornLinks_SandEntities_SandId",
                        column: x => x.SandId,
                        principalTable: "SandEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SandPlumbingLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SandId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlumbingId = table.Column<long>(type: "INTEGER", nullable: false),
                    MatchingPhone = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SandPlumbingLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SandPlumbingLinks_PlumbingEntities_PlumbingId",
                        column: x => x.PlumbingId,
                        principalTable: "PlumbingEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SandPlumbingLinks_SandEntities_SandId",
                        column: x => x.SandId,
                        principalTable: "SandEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaliperEntities_PhoneNumber",
                table: "CaliperEntities",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CaliperEntities_PhoneNumber_Date",
                table: "CaliperEntities",
                columns: new[] { "PhoneNumber", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CornCaliperLinks_CaliperId",
                table: "CornCaliperLinks",
                column: "CaliperId");

            migrationBuilder.CreateIndex(
                name: "IX_CornCaliperLinks_CornId",
                table: "CornCaliperLinks",
                column: "CornId");

            migrationBuilder.CreateIndex(
                name: "IX_CornCaliperLinks_CornId_CaliperId",
                table: "CornCaliperLinks",
                columns: new[] { "CornId", "CaliperId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CornEntities_PhoneNumber",
                table: "CornEntities",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CornEntities_PhoneNumber_Source",
                table: "CornEntities",
                columns: new[] { "PhoneNumber", "Source" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CornPlumbingLinks_CornId",
                table: "CornPlumbingLinks",
                column: "CornId");

            migrationBuilder.CreateIndex(
                name: "IX_CornPlumbingLinks_CornId_PlumbingId",
                table: "CornPlumbingLinks",
                columns: new[] { "CornId", "PlumbingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CornPlumbingLinks_PlumbingId",
                table: "CornPlumbingLinks",
                column: "PlumbingId");

            migrationBuilder.CreateIndex(
                name: "IX_CustardCaliperLinks_CaliperId",
                table: "CustardCaliperLinks",
                column: "CaliperId");

            migrationBuilder.CreateIndex(
                name: "IX_CustardCaliperLinks_CustardId",
                table: "CustardCaliperLinks",
                column: "CustardId");

            migrationBuilder.CreateIndex(
                name: "IX_CustardCaliperLinks_CustardId_CaliperId",
                table: "CustardCaliperLinks",
                columns: new[] { "CustardId", "CaliperId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustardCornLinks_CornId",
                table: "CustardCornLinks",
                column: "CornId");

            migrationBuilder.CreateIndex(
                name: "IX_CustardCornLinks_CustardId",
                table: "CustardCornLinks",
                column: "CustardId");

            migrationBuilder.CreateIndex(
                name: "IX_CustardCornLinks_CustardId_CornId",
                table: "CustardCornLinks",
                columns: new[] { "CustardId", "CornId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustardEntities_PhoneNumber",
                table: "CustardEntities",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CustardEntities_PhoneNumber2",
                table: "CustardEntities",
                column: "PhoneNumber2");

            migrationBuilder.CreateIndex(
                name: "IX_CustardPlumbingLinks_CustardId",
                table: "CustardPlumbingLinks",
                column: "CustardId");

            migrationBuilder.CreateIndex(
                name: "IX_CustardPlumbingLinks_CustardId_PlumbingId",
                table: "CustardPlumbingLinks",
                columns: new[] { "CustardId", "PlumbingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustardPlumbingLinks_PlumbingId",
                table: "CustardPlumbingLinks",
                column: "PlumbingId");

            migrationBuilder.CreateIndex(
                name: "IX_PlumbingCaliperLinks_CaliperId",
                table: "PlumbingCaliperLinks",
                column: "CaliperId");

            migrationBuilder.CreateIndex(
                name: "IX_PlumbingCaliperLinks_PlumbingId",
                table: "PlumbingCaliperLinks",
                column: "PlumbingId");

            migrationBuilder.CreateIndex(
                name: "IX_PlumbingCaliperLinks_PlumbingId_CaliperId",
                table: "PlumbingCaliperLinks",
                columns: new[] { "PlumbingId", "CaliperId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlumbingEntities_PhoneNumber",
                table: "PlumbingEntities",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_PlumbingEntities_PhoneNumber_Source",
                table: "PlumbingEntities",
                columns: new[] { "PhoneNumber", "Source" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SandCaliperLinks_CaliperId",
                table: "SandCaliperLinks",
                column: "CaliperId");

            migrationBuilder.CreateIndex(
                name: "IX_SandCaliperLinks_SandId",
                table: "SandCaliperLinks",
                column: "SandId");

            migrationBuilder.CreateIndex(
                name: "IX_SandCaliperLinks_SandId_CaliperId",
                table: "SandCaliperLinks",
                columns: new[] { "SandId", "CaliperId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SandCornLinks_CornId",
                table: "SandCornLinks",
                column: "CornId");

            migrationBuilder.CreateIndex(
                name: "IX_SandCornLinks_SandId",
                table: "SandCornLinks",
                column: "SandId");

            migrationBuilder.CreateIndex(
                name: "IX_SandCornLinks_SandId_CornId",
                table: "SandCornLinks",
                columns: new[] { "SandId", "CornId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SandEntities_CustardId",
                table: "SandEntities",
                column: "CustardId");

            migrationBuilder.CreateIndex(
                name: "IX_SandPlumbingLinks_PlumbingId",
                table: "SandPlumbingLinks",
                column: "PlumbingId");

            migrationBuilder.CreateIndex(
                name: "IX_SandPlumbingLinks_SandId",
                table: "SandPlumbingLinks",
                column: "SandId");

            migrationBuilder.CreateIndex(
                name: "IX_SandPlumbingLinks_SandId_PlumbingId",
                table: "SandPlumbingLinks",
                columns: new[] { "SandId", "PlumbingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncState_BusinessId",
                table: "SyncState",
                column: "BusinessId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CornCaliperLinks");

            migrationBuilder.DropTable(
                name: "CornPlumbingLinks");

            migrationBuilder.DropTable(
                name: "CustardCaliperLinks");

            migrationBuilder.DropTable(
                name: "CustardCornLinks");

            migrationBuilder.DropTable(
                name: "CustardPlumbingLinks");

            migrationBuilder.DropTable(
                name: "PlumbingCaliperLinks");

            migrationBuilder.DropTable(
                name: "SandCaliperLinks");

            migrationBuilder.DropTable(
                name: "SandCornLinks");

            migrationBuilder.DropTable(
                name: "SandPlumbingLinks");

            migrationBuilder.DropTable(
                name: "SyncState");

            migrationBuilder.DropTable(
                name: "CaliperEntities");

            migrationBuilder.DropTable(
                name: "CornEntities");

            migrationBuilder.DropTable(
                name: "PlumbingEntities");

            migrationBuilder.DropTable(
                name: "SandEntities");

            migrationBuilder.DropTable(
                name: "CustardEntities");
        }
    }
}
