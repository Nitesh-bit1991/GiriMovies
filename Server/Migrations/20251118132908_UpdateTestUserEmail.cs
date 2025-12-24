using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiriMovies.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTestUserEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 18, 13, 29, 7, 781, DateTimeKind.Utc).AddTicks(8128));

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 18, 13, 29, 7, 781, DateTimeKind.Utc).AddTicks(8132));

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 18, 13, 29, 7, 781, DateTimeKind.Utc).AddTicks(8135));

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 18, 13, 29, 7, 781, DateTimeKind.Utc).AddTicks(8138));

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 18, 13, 29, 7, 781, DateTimeKind.Utc).AddTicks(8140));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Email", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 11, 18, 13, 29, 7, 781, DateTimeKind.Utc).AddTicks(8366), "test@GiriMovies.com", new DateTime(2025, 11, 18, 13, 29, 7, 781, DateTimeKind.Utc).AddTicks(8367) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5672));

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5675));

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5677));

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5680));

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5682));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Email", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5868), "test@netflix.com", new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5869) });
        }
    }
}
