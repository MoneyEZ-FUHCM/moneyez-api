using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddSubcategoryToFinancialGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubcategoryId",
                table: "FinancialGoal",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialGoal_SubcategoryId",
                table: "FinancialGoal",
                column: "SubcategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialGoal_Subcategory_SubcategoryId",
                table: "FinancialGoal",
                column: "SubcategoryId",
                principalTable: "Subcategory",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialGoal_Subcategory_SubcategoryId",
                table: "FinancialGoal");

            migrationBuilder.DropIndex(
                name: "IX_FinancialGoal_SubcategoryId",
                table: "FinancialGoal");

            migrationBuilder.DropColumn(
                name: "SubcategoryId",
                table: "FinancialGoal");
        }
    }
}
