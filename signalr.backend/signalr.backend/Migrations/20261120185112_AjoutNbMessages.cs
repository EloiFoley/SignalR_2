using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace signalr.backend.Migrations
{
    /// <inheritdoc />
    public partial class AjoutNbMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NbMessages",
                table: "Channel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Channel",
                keyColumn: "Id",
                keyValue: 1,
                column: "NbMessages",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Channel",
                keyColumn: "Id",
                keyValue: 2,
                column: "NbMessages",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Channel",
                keyColumn: "Id",
                keyValue: 3,
                column: "NbMessages",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NbMessages",
                table: "Channel");
        }
    }
}
