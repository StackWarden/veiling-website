using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddClockLocationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClockLocationId",
                table: "Auctions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClockLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClockLocations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_ClockLocationId",
                table: "Auctions",
                column: "ClockLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_ClockLocations_ClockLocationId",
                table: "Auctions",
                column: "ClockLocationId",
                principalTable: "ClockLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_ClockLocations_ClockLocationId",
                table: "Auctions");

            migrationBuilder.DropTable(
                name: "ClockLocations");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_ClockLocationId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "ClockLocationId",
                table: "Auctions");
        }
    }
}
