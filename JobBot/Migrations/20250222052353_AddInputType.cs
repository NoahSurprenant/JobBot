using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBot.Migrations
{
    /// <inheritdoc />
    public partial class AddInputType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InputType",
                table: "SingleLines",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InputType",
                table: "AutoLines",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InputType",
                table: "SingleLines");

            migrationBuilder.DropColumn(
                name: "InputType",
                table: "AutoLines");
        }
    }
}
