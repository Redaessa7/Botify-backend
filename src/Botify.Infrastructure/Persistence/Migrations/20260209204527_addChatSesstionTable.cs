using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botify.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addChatSesstionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MerchantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExternalChatId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Platform = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LastInteractionAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_ExternalChatId_Platform",
                table: "ChatSessions",
                columns: new[] { "ExternalChatId", "Platform" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatSessions");
        }
    }
}
