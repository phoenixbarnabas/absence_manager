using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class UserManagerRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntraObjectId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Department = table.Column<string>(type: "text", nullable: false),
                    JobTitle = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbsenceRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DateFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    DateTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DecisionComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbsenceRequests_AppUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbsenceRequests_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppUserManagerRelations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserEntraObjectId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ManagerUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ManagerEntraObjectId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SyncedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserManagerRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUserManagerRelations_AppUsers_ManagerUserId",
                        column: x => x.ManagerUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppUserManagerRelations_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Offices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LocationId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offices_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workstations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OfficeId = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    PositionX = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    PositionY = table.Column<decimal>(type: "numeric(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workstations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workstations_Offices_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Offices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfficeBookings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    WorkstationId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", nullable: false),
                    BookingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CancelledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsCancelled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficeBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfficeBookings_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfficeBookings_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "DisplayOrder", "IsActive", "Name" },
                values: new object[] { "loc-Fót", 1, true, "Fót" });

            migrationBuilder.InsertData(
                table: "Offices",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "LocationId", "Name" },
                values: new object[,]
                {
                    { "office-ft-1", "IT Office", 1, true, "loc-Fót", "113 - IT Fejlesztés" },
                    { "office-ft-2", "IT Office", 2, true, "loc-Fót", "110 - IT Üzemeltetés" }
                });

            migrationBuilder.InsertData(
                table: "Workstations",
                columns: new[] { "Id", "Code", "DisplayOrder", "IsActive", "Name", "OfficeId", "PositionX", "PositionY" },
                values: new object[,]
                {
                    { "ws-1", "KL", 1, true, "1", "office-ft-1", 1m, 1m },
                    { "ws-10", "Üres-5", 3, true, "3", "office-ft-2", 3m, 1m },
                    { "ws-2", "GV", 2, true, "2", "office-ft-1", 2m, 1m },
                    { "ws-3", "KI", 3, true, "3", "office-ft-1", 3m, 1m },
                    { "ws-4", "PB", 4, true, "4", "office-ft-1", 1m, 2m },
                    { "ws-5", "üres-1", 5, true, "5", "office-ft-1", 2m, 2m },
                    { "ws-6", "Szp", 6, true, "6", "office-ft-1", 3m, 2m },
                    { "ws-7", "üres-2", 7, true, "7", "office-ft-1", 2m, 3m },
                    { "ws-8", "Üres-3", 1, true, "1", "office-ft-2", 1m, 1m },
                    { "ws-9", "Üres-4", 2, true, "2", "office-ft-2", 2m, 1m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceRequests_ReviewedByUserId",
                table: "AbsenceRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceRequests_User_DateRange_Status",
                table: "AbsenceRequests",
                columns: new[] { "UserId", "DateFrom", "DateTo", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserManagerRelations_ManagerEntraObjectId",
                table: "AppUserManagerRelations",
                column: "ManagerEntraObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserManagerRelations_ManagerUserId",
                table: "AppUserManagerRelations",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserManagerRelations_UserEntraObjectId",
                table: "AppUserManagerRelations",
                column: "UserEntraObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserManagerRelations_UserId_IsActive",
                table: "AppUserManagerRelations",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "UX_AppUserManagerRelations_OneActivePerUser",
                table: "AppUserManagerRelations",
                column: "UserId",
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_EntraObjectId_TenantId",
                table: "AppUsers",
                columns: new[] { "EntraObjectId", "TenantId" },
                unique: true,
                filter: "\"TenantId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfficeBookings_AppUser_BookingDate_IsCancelled",
                table: "OfficeBookings",
                columns: new[] { "UserId", "BookingDate", "IsCancelled" });

            migrationBuilder.CreateIndex(
                name: "IX_OfficeBookings_Workstation_BookingDate_IsCancelled",
                table: "OfficeBookings",
                columns: new[] { "WorkstationId", "BookingDate", "IsCancelled" });

            migrationBuilder.CreateIndex(
                name: "IX_Offices_LocationId_Name",
                table: "Offices",
                columns: new[] { "LocationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workstations_OfficeId_Code",
                table: "Workstations",
                columns: new[] { "OfficeId", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_Workstations_OfficeId_Name",
                table: "Workstations",
                columns: new[] { "OfficeId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbsenceRequests");

            migrationBuilder.DropTable(
                name: "AppUserManagerRelations");

            migrationBuilder.DropTable(
                name: "OfficeBookings");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "Workstations");

            migrationBuilder.DropTable(
                name: "Offices");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
