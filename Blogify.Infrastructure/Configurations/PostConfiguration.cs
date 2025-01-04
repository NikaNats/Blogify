using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
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
        builder.ToTable("Posts");
        builder.HasKey(p => p.Id);
    }

    private static void ConfigureOwnedTypes(EntityTypeBuilder<Post> builder)
    {
        builder.OwnsOne(p => p.Title, title =>
        {
            title.Property(t => t.Value)
                .IsRequired()
                .HasMaxLength(200);
        });

        builder.OwnsOne(p => p.Content, content =>
        {
            content.Property(c => c.Value)
                .IsRequired()
                .HasMaxLength(5000);
        });

        builder.OwnsOne(p => p.Excerpt, excerpt =>
        {
            excerpt.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(500);
        });

        builder.OwnsOne(p => p.Slug, slug =>
        {
            slug.Property(s => s.Value)
                .IsRequired()
                .HasMaxLength(200);
        });
    }

    private static void ConfigureProperties(EntityTypeBuilder<Post> builder)
    {
        builder.Property(p => p.AuthorId)
            .IsRequired();

        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);

        builder.Property(p => p.PublishedAt)
            .IsRequired(false);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Post> builder)
    {
        builder.HasMany(p => p.Comments)
            .WithOne()
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity(j => j.ToTable("PostTags"));

        builder.HasOne<Category>()
            .WithMany(c => c.Posts)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Post> builder)
    {
        builder.HasIndex(p => p.AuthorId);
        builder.HasIndex(p => p.CategoryId);
    }
}