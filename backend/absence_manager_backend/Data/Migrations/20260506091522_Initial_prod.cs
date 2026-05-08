using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial_prod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: "ws-5",
                column: "Code",
                value: "Senki");

            migrationBuilder.UpdateData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: "ws-7",
                column: "Code",
                value: "Senki");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: "ws-5",
                column: "Code",
                value: "F1");

            migrationBuilder.UpdateData(
                table: "Workstations",
                keyColumn: "Id",
                keyValue: "ws-7",
                column: "Code",
                value: "F2");
        }
    }
}
