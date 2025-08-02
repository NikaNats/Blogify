using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blogify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxLockingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "locked_by",
                table: "outbox_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "locked_until_utc",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_locked_until_utc_processed_on_utc",
                table: "outbox_messages",
                columns: new[] { "locked_until_utc", "processed_on_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_locked_until_utc_processed_on_utc",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "locked_by",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "locked_until_utc",
                table: "outbox_messages");
        }
    }
}
