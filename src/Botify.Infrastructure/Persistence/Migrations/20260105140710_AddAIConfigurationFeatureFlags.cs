using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botify.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAIConfigurationFeatureFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoEscalateEnabled",
                table: "AIConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "OrderProcessingEnabled",
                table: "AIConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ProductRecommendationsEnabled",
                table: "AIConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmallTalkEnabled",
                table: "AIConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoEscalateEnabled",
                table: "AIConfigurations");

            migrationBuilder.DropColumn(
                name: "OrderProcessingEnabled",
                table: "AIConfigurations");

            migrationBuilder.DropColumn(
                name: "ProductRecommendationsEnabled",
                table: "AIConfigurations");

            migrationBuilder.DropColumn(
                name: "SmallTalkEnabled",
                table: "AIConfigurations");
        }
    }
}
