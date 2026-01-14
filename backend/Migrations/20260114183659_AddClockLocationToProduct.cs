using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddClockLocationToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClockLocationId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ClockLocationId",
                table: "Products",
                column: "ClockLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ClockLocations_ClockLocationId",
                table: "Products",
                column: "ClockLocationId",
                principalTable: "ClockLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ClockLocations_ClockLocationId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ClockLocationId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ClockLocationId",
                table: "Products");
        }
    }
}
