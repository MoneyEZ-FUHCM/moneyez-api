using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnCodeAndIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialGoal_Subcategory_SubcategoryId",
                table: "FinancialGoal");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Subcategory",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Subcategory",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Category",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Category",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK__Financial__Subcategory__8A4B4B5C",
                table: "FinancialGoal",
                column: "SubcategoryId",
                principalTable: "Subcategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Financial__Subcategory__8A4B4B5C",
                table: "FinancialGoal");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Subcategory");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Subcategory");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Category");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialGoal_Subcategory_SubcategoryId",
                table: "FinancialGoal",
                column: "SubcategoryId",
                principalTable: "Subcategory",
                principalColumn: "Id");
        }
    }
}
