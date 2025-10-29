using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class auctionitemsupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuctionItems",
                table: "AuctionItems");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuctionItems",
                table: "AuctionItems",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionItems_AuctionId_ProductId",
                table: "AuctionItems",
                columns: new[] { "AuctionId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuctionItems",
                table: "AuctionItems");

            migrationBuilder.DropIndex(
                name: "IX_AuctionItems_AuctionId_ProductId",
                table: "AuctionItems");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuctionItems",
                table: "AuctionItems",
                columns: new[] { "AuctionId", "ProductId" });
        }
    }
}
