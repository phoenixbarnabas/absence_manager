using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class OfficeBookingModuleInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Offices",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "OfficeId",
                table: "Workstations",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Workstations",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "LocationId",
                table: "Offices",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Offices",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "WorkstationId",
                table: "OfficeBookings",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "OfficeBookings",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Locations",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "DisplayOrder", "IsActive", "Name" },
                values: new object[] { "1", 1, true, "Budapest" });

            migrationBuilder.InsertData(
                table: "Offices",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "LocationId", "Name" },
                values: new object[] { "1", "Alapértelmezett iroda", 1, true, "1", "Iroda 1" });

            migrationBuilder.InsertData(
                table: "Workstations",
                columns: new[] { "Id", "Code", "DisplayOrder", "IsActive", "Name", "OfficeId", "PositionX", "PositionY" },
                values: new object[,]
                {
                    { "1", "WS-001", 1, true, "1. hely", "1", null, null },
                    { "2", "WS-002", 2, true, "2. hely", "1", null, null },
                    { "3", "WS-003", 3, true, "3. hely", "1", null, null },
                    { "4", "WS-004", 4, true, "4. hely", "1", null, null },
                    { "5", "WS-005", 5, true, "5. hely", "1", null, null },
                    { "6", "WS-006", 6, true, "6. hely", "1", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: "2");

            migrationBuilder.DeleteData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: "3");

            migrationBuilder.DeleteData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: "4");

            migrationBuilder.DeleteData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: "5");

            migrationBuilder.DeleteData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: "6");

            migrationBuilder.DeleteData(
                table: "Offices",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.AlterColumn<int>(
                name: "OfficeId",
                table: "Workstations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Workstations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                table: "Offices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Offices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "WorkstationId",
                table: "OfficeBookings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "OfficeBookings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Locations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "DisplayOrder", "IsActive", "Name" },
                values: new object[] { 1, 1, true, "Budapest" });

            migrationBuilder.InsertData(
                table: "Offices",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "LocationId", "Name" },
                values: new object[] { 1, "Alapértelmezett iroda", 1, true, 1, "Iroda 1" });

            migrationBuilder.InsertData(
                table: "Workstations",
                columns: new[] { "Id", "Code", "DisplayOrder", "IsActive", "Name", "OfficeId", "PositionX", "PositionY" },
                values: new object[,]
                {
                    { 1, "WS-001", 1, true, "1. hely", 1, null, null },
                    { 2, "WS-002", 2, true, "2. hely", 1, null, null },
                    { 3, "WS-003", 3, true, "3. hely", 1, null, null },
                    { 4, "WS-004", 4, true, "4. hely", 1, null, null },
                    { 5, "WS-005", 5, true, "5. hely", 1, null, null },
                    { 6, "WS-006", 6, true, "6. hely", 1, null, null }
                });
        }
    }
}
