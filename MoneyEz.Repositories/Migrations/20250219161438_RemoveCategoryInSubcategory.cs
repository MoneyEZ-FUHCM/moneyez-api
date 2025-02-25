using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryInSubcategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__GroupFund__Group__75A278F5",
                table: "GroupFundLog");

            migrationBuilder.DropForeignKey(
                name: "FK__GroupMemb__Group__787EE5A0",
                table: "GroupMemberLog");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Subcategory");

            migrationBuilder.AddForeignKey(
                name: "FK__GroupFund__Group__75A278F5",
                table: "GroupFundLog",
                column: "GroupId",
                principalTable: "GroupFund",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__GroupMemb__Group__787EE5A0",
                table: "GroupMemberLog",
                column: "GroupMemberId",
                principalTable: "GroupMember",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__GroupFund__Group__75A278F5",
                table: "GroupFundLog");

            migrationBuilder.DropForeignKey(
                name: "FK__GroupMemb__Group__787EE5A0",
                table: "GroupMemberLog");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Subcategory",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK__GroupFund__Group__75A278F5",
                table: "GroupFundLog",
                column: "GroupId",
                principalTable: "GroupFund",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__GroupMemb__Group__787EE5A0",
                table: "GroupMemberLog",
                column: "GroupMemberId",
                principalTable: "GroupMember",
                principalColumn: "Id");
        }
    }
}
