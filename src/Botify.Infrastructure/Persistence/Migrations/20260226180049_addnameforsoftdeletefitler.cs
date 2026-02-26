using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botify.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addnameforsoftdeletefitler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIConfigurations_Merchants_MerchantId",
                table: "AIConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_AspNetUsers_IdentityUserId",
                table: "UserProfiles");

            migrationBuilder.AddForeignKey(
                name: "FK_AIConfigurations_Merchants_MerchantId",
                table: "AIConfigurations",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_AspNetUsers_IdentityUserId",
                table: "UserProfiles",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIConfigurations_Merchants_MerchantId",
                table: "AIConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_AspNetUsers_IdentityUserId",
                table: "UserProfiles");

            migrationBuilder.AddForeignKey(
                name: "FK_AIConfigurations_Merchants_MerchantId",
                table: "AIConfigurations",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_AspNetUsers_IdentityUserId",
                table: "UserProfiles",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
