using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBot.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComboBoxes_ComboBoxOptions_SelectedOptionValue_Label",
                table: "ComboBoxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Radios_RadioOptions_SelectedOptionValue_Label",
                table: "Radios");

            migrationBuilder.RenameColumn(
                name: "SingleLineValue",
                table: "SingleLines",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "SelectedOptionValue",
                table: "Radios",
                newName: "Value");

            migrationBuilder.RenameIndex(
                name: "IX_Radios_SelectedOptionValue_Label",
                table: "Radios",
                newName: "IX_Radios_Value_Label");

            migrationBuilder.RenameColumn(
                name: "OptionValue",
                table: "RadioOptions",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "OptionValue",
                table: "ComboBoxOptions",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "SelectedOptionValue",
                table: "ComboBoxes",
                newName: "Value");

            migrationBuilder.RenameIndex(
                name: "IX_ComboBoxes_SelectedOptionValue_Label",
                table: "ComboBoxes",
                newName: "IX_ComboBoxes_Value_Label");

            migrationBuilder.RenameColumn(
                name: "AutoLineValue",
                table: "AutoLines",
                newName: "Value");

            migrationBuilder.AddForeignKey(
                name: "FK_ComboBoxes_ComboBoxOptions_Value_Label",
                table: "ComboBoxes",
                columns: new[] { "Value", "Label" },
                principalTable: "ComboBoxOptions",
                principalColumns: new[] { "Value", "Label" });

            migrationBuilder.AddForeignKey(
                name: "FK_Radios_RadioOptions_Value_Label",
                table: "Radios",
                columns: new[] { "Value", "Label" },
                principalTable: "RadioOptions",
                principalColumns: new[] { "Value", "Label" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComboBoxes_ComboBoxOptions_Value_Label",
                table: "ComboBoxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Radios_RadioOptions_Value_Label",
                table: "Radios");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "SingleLines",
                newName: "SingleLineValue");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Radios",
                newName: "SelectedOptionValue");

            migrationBuilder.RenameIndex(
                name: "IX_Radios_Value_Label",
                table: "Radios",
                newName: "IX_Radios_SelectedOptionValue_Label");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "RadioOptions",
                newName: "OptionValue");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "ComboBoxOptions",
                newName: "OptionValue");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "ComboBoxes",
                newName: "SelectedOptionValue");

            migrationBuilder.RenameIndex(
                name: "IX_ComboBoxes_Value_Label",
                table: "ComboBoxes",
                newName: "IX_ComboBoxes_SelectedOptionValue_Label");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "AutoLines",
                newName: "AutoLineValue");

            migrationBuilder.AddForeignKey(
                name: "FK_ComboBoxes_ComboBoxOptions_SelectedOptionValue_Label",
                table: "ComboBoxes",
                columns: new[] { "SelectedOptionValue", "Label" },
                principalTable: "ComboBoxOptions",
                principalColumns: new[] { "OptionValue", "Label" });

            migrationBuilder.AddForeignKey(
                name: "FK_Radios_RadioOptions_SelectedOptionValue_Label",
                table: "Radios",
                columns: new[] { "SelectedOptionValue", "Label" },
                principalTable: "RadioOptions",
                principalColumns: new[] { "OptionValue", "Label" });
        }
    }
}
