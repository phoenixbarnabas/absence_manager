using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
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
                table: "AppUsers",
                columns: new[] { "Id", "CreatedAt", "Department", "DisplayName", "Email", "EntraObjectId", "IsActive", "JobTitle", "TenantId" },
                values: new object[,]
                {
                    { "user-1", new DateTime(2026, 3, 27, 8, 30, 0, 0, DateTimeKind.Utc), "IT", "András Bátori", "batori@email.com", "entra-1", true, "Software Engineer", "tenant-1" },
                    { "user-2", new DateTime(2026, 3, 27, 8, 30, 0, 0, DateTimeKind.Utc), "IT", "Fenyvesi Barnabás", "fenyvesi@email.com", "entra-2", true, "GYAKORNOK", "tenant-1" }
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "DisplayOrder", "IsActive", "Name" },
                values: new object[] { "loc-Fót", 1, true, "Fót" });

            migrationBuilder.InsertData(
                table: "Offices",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "LocationId", "Name" },
                values: new object[] { "office-ft-1", "IT fejlesztés", 1, true, "loc-Fót", "IT Office" });

            migrationBuilder.InsertData(
                table: "Workstations",
                columns: new[] { "Id", "Code", "DisplayOrder", "IsActive", "Name", "OfficeId", "PositionX", "PositionY" },
                values: new object[,]
                {
                    { "ws-1", "A1", 1, true, "Desk A1", "office-ft-1", 1m, 1m },
                    { "ws-2", "A2", 2, true, "Desk A2", "office-ft-1", 2m, 1m },
                    { "ws-3", "A3", 3, true, "Desk A3", "office-ft-1", 3m, 1m },
                    { "ws-4", "B1", 4, true, "Desk B1", "office-ft-1", 1m, 2m },
                    { "ws-5", "B2", 5, true, "Desk B2", "office-ft-1", 2m, 2m },
                    { "ws-6", "B3", 6, true, "Desk B3", "office-ft-1", 3m, 2m }
                });

            migrationBuilder.InsertData(
                table: "OfficeBookings",
                columns: new[] { "Id", "BookingDate", "CancelledAtUtc", "CancelledByUserId", "CreatedAtUtc", "CreatedByUserId", "IsCancelled", "UserId", "WorkstationId" },
                values: new object[,]
                {
                    { "booking-1", new DateOnly(2026, 3, 30), null, null, new DateTime(2026, 3, 27, 8, 30, 0, 0, DateTimeKind.Utc), "user-1", false, "user-1", "ws-1" },
                    { "booking-2", new DateOnly(2026, 3, 30), null, null, new DateTime(2026, 3, 27, 8, 30, 0, 0, DateTimeKind.Utc), "user-2", false, "user-2", "ws-2" }
                });

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
                columns: new[] { "OfficeId", "Code" },
                unique: true);

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
