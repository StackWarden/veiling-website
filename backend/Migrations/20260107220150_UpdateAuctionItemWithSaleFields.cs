using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuctionItemWithSaleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[SaleResults]', N'U') IS NOT NULL
                    DROP TABLE [SaleResults];
            ");

            migrationBuilder.AddColumn<int>(
                name: "StatusTemp",
                table: "AuctionItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE [AuctionItems]
                SET [StatusTemp] =
                    CASE [Status]
                        WHEN 'Pending' THEN 0
                        WHEN 'Live' THEN 1
                        WHEN 'Sold' THEN 2
                        WHEN 'Passed' THEN 3
                        ELSE 0
                    END
            ");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AuctionItems");

            migrationBuilder.RenameColumn(
                name: "StatusTemp",
                table: "AuctionItems",
                newName: "Status");

            migrationBuilder.AddColumn<Guid>(
                name: "BuyerId",
                table: "AuctionItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SoldAtUtc",
                table: "AuctionItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoldPrice",
                table: "AuctionItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "BuyerId", table: "AuctionItems");
            migrationBuilder.DropColumn(name: "SoldAtUtc", table: "AuctionItems");
            migrationBuilder.DropColumn(name: "SoldPrice", table: "AuctionItems");

            migrationBuilder.AddColumn<string>(
                name: "StatusTemp",
                table: "AuctionItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.Sql(@"
                UPDATE [AuctionItems]
                SET [StatusTemp] =
                    CASE [Status]
                        WHEN 0 THEN 'Pending'
                        WHEN 1 THEN 'Live'
                        WHEN 2 THEN 'Sold'
                        WHEN 3 THEN 'Passed'
                        ELSE 'Pending'
                    END
            ");

            migrationBuilder.DropColumn(name: "Status", table: "AuctionItems");

            migrationBuilder.RenameColumn(
                name: "StatusTemp",
                table: "AuctionItems",
                newName: "Status");

            migrationBuilder.CreateTable(
                name: "SaleResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuctionItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TotalProceeds = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleResults", x => x.Id);
                });
        }
    }
}
