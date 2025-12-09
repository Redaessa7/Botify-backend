using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botify.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialMediaLinksInShopSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramLink",
                table: "ShopSettings");

            migrationBuilder.AddColumn<string>(
                name: "SocialMediaLinks_Facebook",
                table: "ShopSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialMediaLinks_Instagram",
                table: "ShopSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialMediaLinks_LinkedIn",
                table: "ShopSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialMediaLinks_Twitter",
                table: "ShopSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialMediaLinks_Website",
                table: "ShopSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialMediaLinks_Whatsapp",
                table: "ShopSettings",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SocialMediaLinks_Facebook",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "SocialMediaLinks_Instagram",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "SocialMediaLinks_LinkedIn",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "SocialMediaLinks_Twitter",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "SocialMediaLinks_Website",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "SocialMediaLinks_Whatsapp",
                table: "ShopSettings");

            migrationBuilder.AddColumn<string>(
                name: "TelegramLink",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);
        }
    }
}
