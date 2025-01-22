using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserSetting_UserId",
                table: "UserSetting",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialReport_GroupId",
                table: "FinancialReport",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialReport_UserId",
                table: "FinancialReport",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAndLiability_UserId",
                table: "AssetAndLiability",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK__AssetAndL__UserI__123EB7A3",
                table: "AssetAndLiability",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__FinancialReport__GroupFundId",
                table: "FinancialReport",
                column: "GroupId",
                principalTable: "GroupFund",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__FinancialReport__UserId",
                table: "FinancialReport",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__UserSetting__UserId__12345678",
                table: "UserSetting",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__AssetAndL__UserI__123EB7A3",
                table: "AssetAndLiability");

            migrationBuilder.DropForeignKey(
                name: "FK__FinancialReport__GroupFundId",
                table: "FinancialReport");

            migrationBuilder.DropForeignKey(
                name: "FK__FinancialReport__UserId",
                table: "FinancialReport");

            migrationBuilder.DropForeignKey(
                name: "FK__UserSetting__UserId__12345678",
                table: "UserSetting");

            migrationBuilder.DropIndex(
                name: "IX_UserSetting_UserId",
                table: "UserSetting");

            migrationBuilder.DropIndex(
                name: "IX_FinancialReport_GroupId",
                table: "FinancialReport");

            migrationBuilder.DropIndex(
                name: "IX_FinancialReport_UserId",
                table: "FinancialReport");

            migrationBuilder.DropIndex(
                name: "IX_AssetAndLiability_UserId",
                table: "AssetAndLiability");
        }
    }
}
