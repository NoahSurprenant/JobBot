using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPostings",
                columns: table => new
                {
                    JobPostingID = table.Column<long>(type: "INTEGER", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: false),
                    CompanyLink = table.Column<string>(type: "TEXT", nullable: true),
                    JobTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    IsRepost = table.Column<bool>(type: "INTEGER", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false),
                    Has401k = table.Column<bool>(type: "INTEGER", nullable: false),
                    Dental = table.Column<bool>(type: "INTEGER", nullable: false),
                    Medical = table.Column<bool>(type: "INTEGER", nullable: false),
                    Vision = table.Column<bool>(type: "INTEGER", nullable: false),
                    EasyApply = table.Column<bool>(type: "INTEGER", nullable: false),
                    Applied = table.Column<bool>(type: "INTEGER", nullable: false),
                    NoApplyReason = table.Column<string>(type: "TEXT", nullable: true),
                    DatePulled = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastDatePulled = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostings", x => x.JobPostingID);
                });

            migrationBuilder.CreateTable(
                name: "JobPostingDetails",
                columns: table => new
                {
                    JobPostingID = table.Column<long>(type: "INTEGER", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingDetails", x => x.JobPostingID);
                    table.ForeignKey(
                        name: "FK_JobPostingDetails_JobPostings_JobPostingID",
                        column: x => x.JobPostingID,
                        principalTable: "JobPostings",
                        principalColumn: "JobPostingID");
                });

            migrationBuilder.CreateTable(
                name: "JobPostingQuestions",
                columns: table => new
                {
                    JobPostingID = table.Column<long>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionKind = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionPage = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingQuestions", x => new { x.JobPostingID, x.Label, x.QuestionKind });
                    table.ForeignKey(
                        name: "FK_JobPostingQuestions_JobPostings_JobPostingID",
                        column: x => x.JobPostingID,
                        principalTable: "JobPostings",
                        principalColumn: "JobPostingID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionKind = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => new { x.Value, x.Label, x.QuestionKind });
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    QuestionKind = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    InputType = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => new { x.Label, x.QuestionKind });
                    table.ForeignKey(
                        name: "FK_Questions_Options_Value_Label_QuestionKind",
                        columns: x => new { x.Value, x.Label, x.QuestionKind },
                        principalTable: "Options",
                        principalColumns: new[] { "Value", "Label", "QuestionKind" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingQuestions_Label_QuestionKind",
                table: "JobPostingQuestions",
                columns: new[] { "Label", "QuestionKind" });

            migrationBuilder.CreateIndex(
                name: "IX_Options_Label_QuestionKind",
                table: "Options",
                columns: new[] { "Label", "QuestionKind" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Value_Label_QuestionKind",
                table: "Questions",
                columns: new[] { "Value", "Label", "QuestionKind" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobPostingQuestions_Questions_Label_QuestionKind",
                table: "JobPostingQuestions",
                columns: new[] { "Label", "QuestionKind" },
                principalTable: "Questions",
                principalColumns: new[] { "Label", "QuestionKind" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Options_Questions_Label_QuestionKind",
                table: "Options",
                columns: new[] { "Label", "QuestionKind" },
                principalTable: "Questions",
                principalColumns: new[] { "Label", "QuestionKind" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Options_Questions_Label_QuestionKind",
                table: "Options");

            migrationBuilder.DropTable(
                name: "JobPostingDetails");

            migrationBuilder.DropTable(
                name: "JobPostingQuestions");

            migrationBuilder.DropTable(
                name: "JobPostings");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Options");
        }
    }
}
