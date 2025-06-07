using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogify.Infrastructure.Configurations;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(c => c.Id);

        builder.OwnsOne(c => c.Content, contentBuilder =>
        {
            contentBuilder.Property(c => c.Value)
                .IsRequired()
                .HasMaxLength(1000)
                .HasComment("The content of the comment.");
        });

        builder.Property(c => c.AuthorId)
            .IsRequired()
            .HasComment("The ID of the author who created the comment.");

        builder.Property(c => c.PostId)
            .IsRequired()
            .HasComment("The ID of the post to which the comment belongs.");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasComment("The timestamp when the comment was created.");

        builder.Property(c => c.LastModifiedAt)
            .IsRequired(false)
            .HasComment("The timestamp when the comment was last modified.");

        builder.HasOne<Post>()
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.PostId);
        builder.HasIndex(c => c.AuthorId);
    }
}