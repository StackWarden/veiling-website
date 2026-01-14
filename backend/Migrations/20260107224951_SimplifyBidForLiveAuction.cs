using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyBidForLiveAuction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuctionneerId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "IndividualPrice",
                table: "Bids");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Bids",
                newName: "CreatedAtUtc");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Bids",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "BuyerId",
                table: "Bids",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "AuctionId",
                table: "Bids",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AuctionItemId",
                table: "Bids",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Bids",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Bids_AuctionItemId",
                table: "Bids",
                column: "AuctionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_BuyerId",
                table: "Bids",
                column: "BuyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_AspNetUsers_BuyerId",
                table: "Bids",
                column: "BuyerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_AuctionItems_AuctionItemId",
                table: "Bids",
                column: "AuctionItemId",
                principalTable: "AuctionItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bids_AspNetUsers_BuyerId",
                table: "Bids");

            migrationBuilder.DropForeignKey(
                name: "FK_Bids_AuctionItems_AuctionItemId",
                table: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Bids_AuctionItemId",
                table: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Bids_BuyerId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "AuctionId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "AuctionItemId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Bids");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Bids",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Quantity",
                table: "Bids",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "BuyerId",
                table: "Bids",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "AuctionneerId",
                table: "Bids",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IndividualPrice",
                table: "Bids",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
