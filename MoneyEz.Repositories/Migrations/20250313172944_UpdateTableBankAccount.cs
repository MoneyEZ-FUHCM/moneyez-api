using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableBankAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebhookSecretKey",
                table: "BankAccount",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookUrl",
                table: "BankAccount",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebhookSecretKey",
                table: "BankAccount");

            migrationBuilder.DropColumn(
                name: "WebhookUrl",
                table: "BankAccount");
        }
    }
}
