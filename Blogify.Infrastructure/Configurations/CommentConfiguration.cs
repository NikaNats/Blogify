using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogify.Infrastructure.Configurations;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        // Configure the table name (optional)
        builder.ToTable("Comments");

        // Configure the primary key
        builder.HasKey(c => c.Id);

        // Configure properties
        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(1000); // Adjust the max length as needed

        builder.Property(c => c.AuthorId)
            .IsRequired();

        builder.Property(c => c.PostId)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Configure relationships (if any)
        builder.HasOne<Post>()
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade); // Adjust the delete behavior as needed

        // Configure indexes (if any)
        builder.HasIndex(c => c.PostId);
        builder.HasIndex(c => c.AuthorId);
    }
}