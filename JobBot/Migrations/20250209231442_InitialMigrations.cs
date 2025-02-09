using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoLines",
                columns: table => new
                {
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionPage = table.Column<int>(type: "INTEGER", nullable: false),
                    AutoLineValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoLines", x => x.Label);
                });

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
                name: "SingleLines",
                columns: table => new
                {
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionPage = table.Column<int>(type: "INTEGER", nullable: false),
                    SingleLineValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleLines", x => x.Label);
                });

            migrationBuilder.CreateTable(
                name: "JobPostingAutoLines",
                columns: table => new
                {
                    JobPostingID = table.Column<long>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingAutoLines", x => new { x.JobPostingID, x.Label });
                    table.ForeignKey(
                        name: "FK_JobPostingAutoLines_AutoLines_Label",
                        column: x => x.Label,
                        principalTable: "AutoLines",
                        principalColumn: "Label",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobPostingAutoLines_JobPostings_JobPostingID",
                        column: x => x.JobPostingID,
                        principalTable: "JobPostings",
                        principalColumn: "JobPostingID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPostingSingleLines",
                columns: table => new
                {
                    JobPostingID = table.Column<long>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingSingleLines", x => new { x.JobPostingID, x.Label });
                    table.ForeignKey(
                        name: "FK_JobPostingSingleLines_JobPostings_JobPostingID",
                        column: x => x.JobPostingID,
                        principalTable: "JobPostings",
                        principalColumn: "JobPostingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobPostingSingleLines_SingleLines_Label",
                        column: x => x.Label,
                        principalTable: "SingleLines",
                        principalColumn: "Label",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComboBoxes",
                columns: table => new
                {
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionPage = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectedOptionValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboBoxes", x => x.Label);
                });

            migrationBuilder.CreateTable(
                name: "ComboBoxOptions",
                columns: table => new
                {
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    OptionValue = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboBoxOptions", x => new { x.OptionValue, x.Label });
                    table.ForeignKey(
                        name: "FK_ComboBoxOptions_ComboBoxes_Label",
                        column: x => x.Label,
                        principalTable: "ComboBoxes",
                        principalColumn: "Label",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPostingComboBoxes",
                columns: table => new
                {
                    JobPostingID = table.Column<long>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostingComboBoxes", x => new { x.JobPostingID, x.Label });
                    table.ForeignKey(
                        name: "FK_JobPostingComboBoxes_ComboBoxes_Label",
                        column: x => x.Label,
                        principalTable: "ComboBoxes",
                        principalColumn: "Label",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobPostingComboBoxes_JobPostings_JobPostingID",
                        column: x => x.JobPostingID,
                        principalTable: "JobPostings",
                        principalColumn: "JobPostingID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComboBoxes_SelectedOptionValue_Label",
                table: "ComboBoxes",
                columns: new[] { "SelectedOptionValue", "Label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComboBoxOptions_Label",
                table: "ComboBoxOptions",
                column: "Label");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingAutoLines_Label",
                table: "JobPostingAutoLines",
                column: "Label");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingComboBoxes_Label",
                table: "JobPostingComboBoxes",
                column: "Label");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostingSingleLines_Label",
                table: "JobPostingSingleLines",
                column: "Label");

            migrationBuilder.AddForeignKey(
                name: "FK_ComboBoxes_ComboBoxOptions_SelectedOptionValue_Label",
                table: "ComboBoxes",
                columns: new[] { "SelectedOptionValue", "Label" },
                principalTable: "ComboBoxOptions",
                principalColumns: new[] { "OptionValue", "Label" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComboBoxes_ComboBoxOptions_SelectedOptionValue_Label",
                table: "ComboBoxes");

            migrationBuilder.DropTable(
                name: "JobPostingAutoLines");

            migrationBuilder.DropTable(
                name: "JobPostingComboBoxes");

            migrationBuilder.DropTable(
                name: "JobPostingSingleLines");

            migrationBuilder.DropTable(
                name: "AutoLines");

            migrationBuilder.DropTable(
                name: "JobPostings");

            migrationBuilder.DropTable(
                name: "SingleLines");

            migrationBuilder.DropTable(
                name: "ComboBoxOptions");

            migrationBuilder.DropTable(
                name: "ComboBoxes");
        }
    }
}
