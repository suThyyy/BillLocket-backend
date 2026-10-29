using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillLocket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameChangedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UsernameChangedAt",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsernameChangedAt",
                table: "AspNetUsers");
        }
    }
}
