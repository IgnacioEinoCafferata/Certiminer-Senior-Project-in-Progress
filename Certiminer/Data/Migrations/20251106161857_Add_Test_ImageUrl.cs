using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Certiminer.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Test_ImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Tests",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Tests");
        }

    }
}
