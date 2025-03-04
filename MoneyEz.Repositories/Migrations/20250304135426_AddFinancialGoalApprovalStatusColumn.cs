using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialGoalApprovalStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "FinancialGoal",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "FinancialGoal");
        }
    }
}
