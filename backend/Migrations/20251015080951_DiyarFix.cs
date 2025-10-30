using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class DiyarFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuctionItems",
                table: "AuctionItems");

            migrationBuilder.DropIndex(
                name: "IX_AuctionItems_AuctionId",
                table: "AuctionItems");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuctionItems",
                table: "AuctionItems",
                columns: new[] { "AuctionId", "ProductId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuctionItems",
                table: "AuctionItems");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuctionItems",
                table: "AuctionItems",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionItems_AuctionId",
                table: "AuctionItems",
                column: "AuctionId");
        }
    }
}
