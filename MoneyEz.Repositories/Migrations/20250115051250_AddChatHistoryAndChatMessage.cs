using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddChatHistoryAndChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "ChatHistory");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ChatHistory",
                newName: "ConservationName");

            migrationBuilder.AddColumn<int>(
                name: "RoomNo",
                table: "ChatHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ChatMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChatHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChatMess__3214EC07F429B993", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessage_ChatHistory",
                        column: x => x.ChatHistoryId,
                        principalTable: "ChatHistory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_UserId",
                table: "ChatHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_ChatHistoryId",
                table: "ChatMessage",
                column: "ChatHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatHistory_User",
                table: "ChatHistory",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatHistory_User",
                table: "ChatHistory");

            migrationBuilder.DropTable(
                name: "ChatMessage");

            migrationBuilder.DropIndex(
                name: "IX_ChatHistory_UserId",
                table: "ChatHistory");

            migrationBuilder.DropColumn(
                name: "RoomNo",
                table: "ChatHistory");

            migrationBuilder.RenameColumn(
                name: "ConservationName",
                table: "ChatHistory",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "ChatHistory",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
