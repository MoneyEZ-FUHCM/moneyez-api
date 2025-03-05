using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransCateUserSpendingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "UserSpendingModel",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserSpendingModelId",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Category",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserSpendingModel");

            migrationBuilder.DropColumn(
                name: "UserSpendingModelId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Category");
        }
    }
}
