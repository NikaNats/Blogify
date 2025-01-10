using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogify.Infrastructure.Configurations;

internal sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        ConfigureTableAndKeys(builder);
        ConfigureOwnedTypes(builder);
        ConfigureProperties(builder);
        ConfigureRelationships(builder);
        ConfigureIndexes(builder);
    }

    private static void ConfigureTableAndKeys(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");
        builder.HasKey(p => p.Id);
    }

    private static void ConfigureOwnedTypes(EntityTypeBuilder<Post> builder)
    {
        builder.OwnsOne(p => p.Title, title =>
        {
            title.Property(t => t.Value)
                .HasColumnName("title")
                .IsRequired()
                .HasMaxLength(200)
                .HasComment("The title of the post.");
        });

        builder.OwnsOne(p => p.Content, content =>
        {
            content.Property(c => c.Value)
                .HasColumnName("content")
                .IsRequired()
                .HasMaxLength(5000)
                .HasComment("The content of the post.");
        });

        builder.OwnsOne(p => p.Excerpt, excerpt =>
        {
            excerpt.Property(e => e.Value)
                .HasColumnName("excerpt")
                .IsRequired()
                .HasMaxLength(500)
                .HasComment("A short excerpt summarizing the post.");
        });

        builder.OwnsOne(p => p.Slug, slug =>
        {
            slug.Property(s => s.Value)
                .HasColumnName("slug")
                .IsRequired()
                .HasMaxLength(200)
                .HasComment("The URL-friendly slug for the post.");
        });
    }

    private static void ConfigureProperties(EntityTypeBuilder<Post> builder)
    {
        builder.Property(p => p.AuthorId)
            .IsRequired()
            .HasComment("The ID of the author who created the post.");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasComment("The timestamp when the post was created.");

        builder.Property(p => p.LastModifiedAt)
            .IsRequired(false)
            .HasComment("The timestamp when the post was last modified.");

        builder.Property(p => p.PublishedAt)
            .IsRequired(false)
            .HasComment("The timestamp when the post was published.");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasComment("The current status of the post (e.g., Draft, Published, Archived).");
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Post> builder)
    {
        builder.HasMany(p => p.Comments)
            .WithOne()
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity<Dictionary<string, object>>(
                "posttags",
                j => j.HasOne<Tag>()
                    .WithMany()
                    .HasForeignKey("tag_id") // Match the database column name
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Post>()
                    .WithMany()
                    .HasForeignKey("post_id") // Match the database column name
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("posttags", "blog"); // Ensure the schema matches the database
                    j.HasKey("post_id", "tag_id"); // Define the composite primary key
                });

        builder.HasMany(p => p.Categories)
            .WithMany(c => c.Posts)
            .UsingEntity<Dictionary<string, object>>(
                "postcategories",
                j => j.HasOne<Category>()
                    .WithMany()
                    .HasForeignKey("category_id") // Match the database column name
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Post>()
                    .WithMany()
                    .HasForeignKey("post_id") // Match the database column name
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("postcategories", "blog"); // Ensure the schema matches the database
                    j.HasKey("post_id", "category_id"); // Define the composite primary key
                });
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Post> builder)
    {
        builder.HasIndex(p => p.AuthorId);
    }
}