using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyEz.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizIntoNoSQLStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__AnswerOption__QuestionId",
                table: "AnswerOption");

            migrationBuilder.DropForeignKey(
                name: "FK__Question__QuizId",
                table: "Question");

            migrationBuilder.DropForeignKey(
                name: "FK__UserQuizAnswer__AnswerOptionId",
                table: "UserQuizAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK__UserQuizAnswer__UserQuizResultId",
                table: "UserQuizAnswer");

            migrationBuilder.DropPrimaryKey(
                name: "PK__UserQuizAnswer__3214EC07",
                table: "UserQuizAnswer");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Question__3214EC07",
                table: "Question");

            migrationBuilder.DropPrimaryKey(
                name: "PK__AnswerOption__3214EC07",
                table: "AnswerOption");

            migrationBuilder.AddColumn<string>(
                name: "AnswersJson",
                table: "UserQuizResult",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuizVersion",
                table: "UserQuizResult",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "RecurringTransaction",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "RecurringTransaction",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "RecurringTransaction",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<int>(
                name: "FrequencyType",
                table: "RecurringTransaction",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "RecurringTransaction",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionsJson",
                table: "Quiz",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Quiz",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserQuizAnswer",
                table: "UserQuizAnswer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Question",
                table: "Question",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnswerOption",
                table: "AnswerOption",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerOption_Question_QuestionId",
                table: "AnswerOption",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Question_Quiz_QuizId",
                table: "Question",
                column: "QuizId",
                principalTable: "Quiz",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAnswer_AnswerOption_AnswerOptionId",
                table: "UserQuizAnswer",
                column: "AnswerOptionId",
                principalTable: "AnswerOption",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizAnswer_UserQuizResult_UserQuizResultId",
                table: "UserQuizAnswer",
                column: "UserQuizResultId",
                principalTable: "UserQuizResult",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerOption_Question_QuestionId",
                table: "AnswerOption");

            migrationBuilder.DropForeignKey(
                name: "FK_Question_Quiz_QuizId",
                table: "Question");

            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizAnswer_AnswerOption_AnswerOptionId",
                table: "UserQuizAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizAnswer_UserQuizResult_UserQuizResultId",
                table: "UserQuizAnswer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserQuizAnswer",
                table: "UserQuizAnswer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Question",
                table: "Question");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnswerOption",
                table: "AnswerOption");

            migrationBuilder.DropColumn(
                name: "AnswersJson",
                table: "UserQuizResult");

            migrationBuilder.DropColumn(
                name: "QuizVersion",
                table: "UserQuizResult");

            migrationBuilder.DropColumn(
                name: "QuestionsJson",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Quiz");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "RecurringTransaction",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "RecurringTransaction",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "StartDate",
                table: "RecurringTransaction",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "FrequencyType",
                table: "RecurringTransaction",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EndDate",
                table: "RecurringTransaction",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__UserQuizAnswer__3214EC07",
                table: "UserQuizAnswer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Question__3214EC07",
                table: "Question",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__AnswerOption__3214EC07",
                table: "AnswerOption",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__AnswerOption__QuestionId",
                table: "AnswerOption",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__Question__QuizId",
                table: "Question",
                column: "QuizId",
                principalTable: "Quiz",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__UserQuizAnswer__AnswerOptionId",
                table: "UserQuizAnswer",
                column: "AnswerOptionId",
                principalTable: "AnswerOption",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__UserQuizAnswer__UserQuizResultId",
                table: "UserQuizAnswer",
                column: "UserQuizResultId",
                principalTable: "UserQuizResult",
                principalColumn: "Id");
        }
    }
}
