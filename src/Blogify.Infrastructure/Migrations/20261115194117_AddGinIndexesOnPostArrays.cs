using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blogify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGinIndexesOnPostArrays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_posts_category_ids",
                table: "posts",
                column: "category_ids")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_posts_tag_ids",
                table: "posts",
                column: "tag_ids")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_posts_category_ids",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_posts_tag_ids",
                table: "posts");
        }
    }
}
