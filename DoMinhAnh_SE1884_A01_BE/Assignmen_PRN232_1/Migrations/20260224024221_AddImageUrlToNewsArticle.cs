using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignmen_PRN232__.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToNewsArticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Removed DropTable AuditLog

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "NewsArticle",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "NewsArticle");

            // Removed CreateTable AuditLog        
        }
    }
}
