using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class addSpeciesUpdateProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuctionDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ClockLocation",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Species",
                table: "Products");

            migrationBuilder.AddColumn<Guid>(
                name: "SpeciesId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Species",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LatinName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Family = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GrowthType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPerennial = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Species", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_SpeciesId",
                table: "Products",
                column: "SpeciesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Species_SpeciesId",
                table: "Products",
                column: "SpeciesId",
                principalTable: "Species",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Species_SpeciesId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Species");

            migrationBuilder.DropIndex(
                name: "IX_Products_SpeciesId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SpeciesId",
                table: "Products");

            migrationBuilder.AddColumn<DateOnly>(
                name: "AuctionDate",
                table: "Products",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClockLocation",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Species",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
