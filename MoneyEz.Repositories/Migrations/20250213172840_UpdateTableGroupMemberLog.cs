using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableGroupMemberLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangedAt",
                table: "GroupMemberLog");

            migrationBuilder.DropColumn(
                name: "ChangedAt",
                table: "GroupFundLog");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notification",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notification",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "Notification",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Href",
                table: "Notification",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChangeDiscription",
                table: "GroupMemberLog",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Href",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "ChangeDiscription",
                table: "GroupMemberLog");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notification",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notification",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "Notification",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<DateTime>(
                name: "ChangedAt",
                table: "GroupMemberLog",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "ChangedAt",
                table: "GroupFundLog",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())");
        }
    }
}
