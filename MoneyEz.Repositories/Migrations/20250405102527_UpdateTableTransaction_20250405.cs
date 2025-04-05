using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableTransaction_20250405 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountBankName",
                table: "Transaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountBankNumber",
                table: "Transaction",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BankTransactionDate",
                table: "Transaction",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankTransactionId",
                table: "Transaction",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountBankName",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "AccountBankNumber",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "BankTransactionDate",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "BankTransactionId",
                table: "Transaction");
        }
    }
}
