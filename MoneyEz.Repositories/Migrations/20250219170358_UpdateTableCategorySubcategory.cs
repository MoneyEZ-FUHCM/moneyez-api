using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableCategorySubcategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CategorySubcategory",
                table: "CategorySubcategory");

            migrationBuilder.AddPrimaryKey(
                name: "PK__CategorySubcategory",
                table: "CategorySubcategory",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CategorySubcategory_CategoryId",
                table: "CategorySubcategory",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK__CategorySubcategory",
                table: "CategorySubcategory");

            migrationBuilder.DropIndex(
                name: "IX_CategorySubcategory_CategoryId",
                table: "CategorySubcategory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategorySubcategory",
                table: "CategorySubcategory",
                columns: new[] { "CategoryId", "SubcategoryId" });
        }
    }
}
