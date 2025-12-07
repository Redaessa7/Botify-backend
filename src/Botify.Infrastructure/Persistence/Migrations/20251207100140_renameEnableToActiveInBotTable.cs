using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botify.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class renameEnableToActiveInBotTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Enabled",
                table: "Bots",
                newName: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Active",
                table: "Bots",
                newName: "Enabled");
        }
    }
}
