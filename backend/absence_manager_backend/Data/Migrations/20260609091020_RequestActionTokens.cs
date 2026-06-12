using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RequestActionTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AbsenceRequestActionTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AbsenceRequestId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ManagerUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceRequestActionTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbsenceRequestActionTokens_AbsenceRequests_AbsenceRequestId",
                        column: x => x.AbsenceRequestId,
                        principalTable: "AbsenceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbsenceRequestActionTokens_AppUsers_ManagerUserId",
                        column: x => x.ManagerUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceRequestActionTokens_AbsenceRequestId",
                table: "AbsenceRequestActionTokens",
                column: "AbsenceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceRequestActionTokens_ExpiresAtUtc",
                table: "AbsenceRequestActionTokens",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceRequestActionTokens_ManagerUserId",
                table: "AbsenceRequestActionTokens",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceRequestActionTokens_Request_Action_IsUsed",
                table: "AbsenceRequestActionTokens",
                columns: new[] { "AbsenceRequestId", "Action", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "UX_AbsenceRequestActionTokens_TokenHash",
                table: "AbsenceRequestActionTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbsenceRequestActionTokens");
        }
    }
}
