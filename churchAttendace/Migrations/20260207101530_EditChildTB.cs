using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace churchAttendace.Migrations
{
    /// <inheritdoc />
    public partial class EditChildTB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastGiftDeliveredAt",
                table: "Children",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastGiftDeliveredAt",
                table: "Children");
        }
    }
}
