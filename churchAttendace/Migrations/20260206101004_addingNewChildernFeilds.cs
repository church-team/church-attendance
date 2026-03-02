using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace churchAttendace.Migrations
{
    /// <inheritdoc />
    public partial class addingNewChildernFeilds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Children",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "Children",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "Children");
        }
    }
}
