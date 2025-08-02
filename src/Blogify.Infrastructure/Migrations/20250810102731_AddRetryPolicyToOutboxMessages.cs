using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blogify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRetryPolicyToOutboxMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "attempts",
                table: "outbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_retry_utc",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on_utc_next_retry_utc_attempts",
                table: "outbox_messages",
                columns: new[] { "processed_on_utc", "next_retry_utc", "attempts" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_processed_on_utc_next_retry_utc_attempts",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "attempts",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "next_retry_utc",
                table: "outbox_messages");
        }
    }
}
