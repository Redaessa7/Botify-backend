using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botify.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomerAndSupportRequestSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "SupportRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlatformContact",
                table: "SupportRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Messenger_ChatId",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Messenger_Link",
                table: "Customers",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telegram_ChatId",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telegram_Link",
                table: "Customers",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uuid",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp_ChatId",
                table: "Customers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp_Link",
                table: "Customers",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Uuid",
                table: "Customers",
                column: "Uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Uuid",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "PlatformContact",
                table: "SupportRequests");

            migrationBuilder.DropColumn(
                name: "Messenger_ChatId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Messenger_Link",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Telegram_ChatId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Telegram_Link",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Uuid",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "WhatsApp_ChatId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "WhatsApp_Link",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Customers",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Platform",
                table: "Customers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
