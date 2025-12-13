using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botify.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShopSettingsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ShopSettings",
                table: "ShopSettings");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "ShopSettings",
                newName: "LogoImageUrl");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ShopSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "BusinessEmail",
                table: "ShopSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessPhoneNumber",
                table: "ShopSettings",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "ShopSettings",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeliveryEnabled",
                table: "ShopSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ShopSettings",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndustryName",
                table: "ShopSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                table: "ShopSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumOrder",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Friday_Close",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OpeningHours_Friday_IsOpen",
                table: "ShopSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Friday_Open",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Monday_Close",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OpeningHours_Monday_IsOpen",
                table: "ShopSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Monday_Open",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Saturday_Close",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OpeningHours_Saturday_IsOpen",
                table: "ShopSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Saturday_Open",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Sunday_Close",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OpeningHours_Sunday_IsOpen",
                table: "ShopSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Sunday_Open",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Thursday_Close",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OpeningHours_Thursday_IsOpen",
                table: "ShopSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Thursday_Open",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Tuesday_Close",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OpeningHours_Tuesday_IsOpen",
                table: "ShopSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Tuesday_Open",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Wednesday_Close",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OpeningHours_Wednesday_IsOpen",
                table: "ShopSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "OpeningHours_Wednesday_Open",
                table: "ShopSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PickupEnabled",
                table: "ShopSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "ShopSettings",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShopSettings",
                table: "ShopSettings",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSettings_MerchantId",
                table: "ShopSettings",
                column: "MerchantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ShopSettings",
                table: "ShopSettings");

            migrationBuilder.DropIndex(
                name: "IX_ShopSettings_MerchantId",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "BusinessEmail",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "BusinessPhoneNumber",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "DeliveryEnabled",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "IndustryName",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "MinimumOrder",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Friday_Close",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Friday_IsOpen",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Friday_Open",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Monday_Close",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Monday_IsOpen",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Monday_Open",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Saturday_Close",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Saturday_IsOpen",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Saturday_Open",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Sunday_Close",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Sunday_IsOpen",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Sunday_Open",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Thursday_Close",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Thursday_IsOpen",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Thursday_Open",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Tuesday_Close",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Tuesday_IsOpen",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Tuesday_Open",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Wednesday_Close",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Wednesday_IsOpen",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "OpeningHours_Wednesday_Open",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "PickupEnabled",
                table: "ShopSettings");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "ShopSettings");

            migrationBuilder.RenameColumn(
                name: "LogoImageUrl",
                table: "ShopSettings",
                newName: "ImageUrl");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShopSettings",
                table: "ShopSettings",
                column: "MerchantId");
        }
    }
}
