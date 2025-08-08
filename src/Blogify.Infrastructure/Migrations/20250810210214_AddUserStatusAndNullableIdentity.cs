using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blogify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStatusAndNullableIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "identity_id",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "The lifecycle status of the user (0=Pending,1=Active)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "identity_id",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
