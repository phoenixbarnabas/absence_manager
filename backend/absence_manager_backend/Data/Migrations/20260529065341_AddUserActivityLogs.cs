using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActivityLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserActivityLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ActorEntraObjectId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Outcome = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RequestPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_Action",
                table: "UserActivityLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_ActorUserId",
                table: "UserActivityLogs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_CreatedAtUtc",
                table: "UserActivityLogs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_EntityType_EntityId",
                table: "UserActivityLogs",
                columns: new[] { "EntityType", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActivityLogs");
        }
    }
}
