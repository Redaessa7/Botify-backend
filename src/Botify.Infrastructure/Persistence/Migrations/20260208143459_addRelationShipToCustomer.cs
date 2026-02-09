using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botify.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addRelationShipToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Customers_MerchantId",
                table: "Customers",
                column: "MerchantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Merchants_MerchantId",
                table: "Customers",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Merchants_MerchantId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_MerchantId",
                table: "Customers");
        }
    }
}
