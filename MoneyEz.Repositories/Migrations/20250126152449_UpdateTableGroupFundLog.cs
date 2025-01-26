using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableGroupFundLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Action",
                table: "GroupFundLog",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "GroupFundLog");
        }
    }
}
