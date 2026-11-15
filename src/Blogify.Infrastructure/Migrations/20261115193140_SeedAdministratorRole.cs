using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blogify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdministratorRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "name" },
                values: new object[] { 2, "Administrator" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 2);
        }
    }
}
