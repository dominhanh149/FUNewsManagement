using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnalyticsAPI.Migrations
{
    /// <inheritdoc />
    public partial class TenMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    CategoryID = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryDesciption = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ParentCategoryID = table.Column<short>(type: "smallint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.CategoryID);
                    table.ForeignKey(
                        name: "FK_Category_Category",
                        column: x => x.ParentCategoryID,
                        principalTable: "Category",
                        principalColumn: "CategoryID");
                });

            migrationBuilder.CreateTable(
                name: "SystemAccount",
                columns: table => new
                {
                    AccountID = table.Column<short>(type: "smallint", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccountEmail = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    AccountRole = table.Column<int>(type: "int", nullable: true),
                    AccountPassword = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAccount", x => x.AccountID);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    TagID = table.Column<int>(type: "int", nullable: false),
                    TagName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HashTag", x => x.TagID);
                });

            migrationBuilder.CreateTable(
                name: "NewsArticle",
                columns: table => new
                {
                    NewsArticleID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NewsTitle = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Headline = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    NewsContent = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    NewsSource = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CategoryID = table.Column<short>(type: "smallint", nullable: true),
                    NewsStatus = table.Column<bool>(type: "bit", nullable: true),
                    CreatedByID = table.Column<short>(type: "smallint", nullable: true),
                    UpdatedByID = table.Column<short>(type: "smallint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticle", x => x.NewsArticleID);
                    table.ForeignKey(
                        name: "FK_NewsArticle_Category",
                        column: x => x.CategoryID,
                        principalTable: "Category",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsArticle_SystemAccount",
                        column: x => x.CreatedByID,
                        principalTable: "SystemAccount",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<short>(type: "smallint", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_SystemAccount_AccountId",
                        column: x => x.AccountId,
                        principalTable: "SystemAccount",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsTag",
                columns: table => new
                {
                    NewsArticleID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TagID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsTag", x => new { x.NewsArticleID, x.TagID });
                    table.ForeignKey(
                        name: "FK_NewsTag_NewsArticle",
                        column: x => x.NewsArticleID,
                        principalTable: "NewsArticle",
                        principalColumn: "NewsArticleID");
                    table.ForeignKey(
                        name: "FK_NewsTag_Tag",
                        column: x => x.TagID,
                        principalTable: "Tag",
                        principalColumn: "TagID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Category_ParentCategoryID",
                table: "Category",
                column: "ParentCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_CategoryID",
                table: "NewsArticle",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticle_CreatedByID",
                table: "NewsArticle",
                column: "CreatedByID");

            migrationBuilder.CreateIndex(
                name: "IX_NewsTag_TagID",
                table: "NewsTag",
                column: "TagID");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_AccountId",
                table: "RefreshToken",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsTag");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "NewsArticle");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "SystemAccount");
        }
    }
}
